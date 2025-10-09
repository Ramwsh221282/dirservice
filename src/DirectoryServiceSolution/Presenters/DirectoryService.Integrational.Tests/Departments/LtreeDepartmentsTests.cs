using System.Data.Common;
using Dapper;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using DirectoryService.Integrational.Tests.Locations;
using DirectoryService.WebApi.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests.Departments;

public sealed class DepartmentPoco
{
    public Guid Id { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime Created_At { get; }
    public DateTime Deleted_At { get; }
    public DateTime Updated_At { get; }
    public string Path { get; }
    public short Depth { get; }
    public int Childrens_Count { get; }
}

public sealed class LtreeDepartmentsTests : IClassFixture<RealDatabaseTestApplicationFactory>
{
    private readonly DepartmentsTestsHelper _departmentsTests;
    private readonly LocationsTestsHelper _locationsTests;
    private readonly IServiceProvider _services;

    public LtreeDepartmentsTests(RealDatabaseTestApplicationFactory factory)
    {
        _departmentsTests = new DepartmentsTestsHelper(factory);
        _locationsTests = new LocationsTestsHelper(factory);
        _services = factory.Services;
    }

    [Fact]
    private async Task Create_Departments_Family_Success()
    {
        await CreateDepartmentFamily();
    }

    [Fact]
    private async Task Create_Departments_Family_Ensure_Created_Success()
    {
        const string sql = "SELECT * FROM departments";
        await CreateDepartmentFamily();
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        await using ServiceDbContext db = scope.GetService<ServiceDbContext>();
        DbConnection connection = db.Database.GetDbConnection();
        DepartmentPoco[] items = (await connection.QueryAsync<DepartmentPoco>(sql)).ToArray();
        Assert.NotEmpty(items);
    }

    private async Task CreateDepartmentFamily()
    {
        Guid locationId = await _locationsTests.CreateNewLocation(
            "Test Location",
            "Test/Location",
            ["Some", "Big", "City"]
        );

        Guid rootId = await _departmentsTests.CreateNewDepartment(
            "Department A",
            "department-a",
            [locationId]
        );
        await _departmentsTests.CreateNewDepartment(
            "Department B",
            "department-b",
            [locationId],
            rootId
        );
        Result<Guid> cId = await _departmentsTests.CreateNewDepartment(
            "Department C",
            "department-c",
            [locationId],
            rootId
        );
        await _departmentsTests.CreateNewDepartment(
            "Department D",
            "department-d",
            [locationId],
            cId
        );
    }
}
