using System.Data;
using Dapper;
using DirectoryService.Infrastructure.PostgreSQL.Seeding;
using DirectoryService.WebApi.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Integrational.Tests;

public sealed class SeedingTests : IClassFixture<TestApplicationFactory>, IAsyncLifetime
{
    private const int ExpectedLocationsCount = 14;

    private readonly TestApplicationFactory _factory;

    public SeedingTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabase();
        await _factory.Services.RunSeeders();
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Seeding_Fills_All_Tables()
    {
        Assert.Equal(ExpectedLocationsCount, await CountRows("locations"));
        Assert.True(await CountRows("departments") > 0, "Подразделения не засеяны.");
        Assert.True(await CountRows("positions") > 0, "Должности не засеяны.");
        Assert.True(
            await CountRows("department_locations") > 0,
            "Связи подразделений с локациями не засеяны."
        );
        Assert.True(
            await CountRows("department_positions") > 0,
            "Связи подразделений с должностями не засеяны."
        );
    }

    [Fact]
    public async Task Seeded_Departments_Have_Locations()
    {
        int orphans = await CountScalar(
            """
            SELECT COUNT(*)
            FROM departments d
            WHERE NOT EXISTS (
                SELECT 1 FROM department_locations dl WHERE dl.department_id = d.id
            )
            """
        );

        Assert.Equal(0, orphans);
    }

    [Fact]
    public async Task Seeded_Positions_Have_Departments()
    {
        int orphans = await CountScalar(
            """
            SELECT COUNT(*)
            FROM positions p
            WHERE NOT EXISTS (
                SELECT 1 FROM department_positions dp WHERE dp.position_id = p.id
            )
            """
        );

        Assert.Equal(0, orphans);
    }

    [Fact]
    public async Task Seeded_Departments_Contain_Hierarchy()
    {
        int children = await CountScalar(
            "SELECT COUNT(*) FROM departments WHERE parent_id IS NOT NULL"
        );

        Assert.True(children > 0, "В засеянных подразделениях нет иерархии.");

        int depthMismatch = await CountScalar(
            "SELECT COUNT(*) FROM departments WHERE nlevel(path) <> depth + 1"
        );

        Assert.Equal(0, depthMismatch);
    }

    [Fact]
    public async Task Seeded_Child_Departments_Have_Consistent_Paths()
    {
        int broken = await CountScalar(
            """
            SELECT COUNT(*)
            FROM departments child
            INNER JOIN departments parent ON child.parent_id = parent.id
            WHERE NOT (child.path <@ parent.path)
            """
        );

        Assert.Equal(0, broken);
    }

    [Fact]
    public async Task Seeding_Is_Idempotent()
    {
        int locationsAfterFirstRun = await CountRows("locations");
        int departmentsAfterFirstRun = await CountRows("departments");
        int positionsAfterFirstRun = await CountRows("positions");

        await _factory.Services.RunSeeders();

        Assert.Equal(locationsAfterFirstRun, await CountRows("locations"));
        Assert.Equal(departmentsAfterFirstRun, await CountRows("departments"));
        Assert.Equal(positionsAfterFirstRun, await CountRows("positions"));
    }

    private async Task<int> CountRows(string table)
    {
        return await CountScalar($"SELECT COUNT(*) FROM {table}");
    }

    private async Task<int> CountScalar(string sql)
    {
        await using AsyncServiceScope scope = _factory.Services.CreateAsyncScope();
        IDbConnection connection = scope.GetService<IDbConnection>();
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql));
    }
}
