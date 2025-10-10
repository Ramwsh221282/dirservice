using System.Data.Common;
using Dapper;
using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Departments.Pocos;
using DirectoryService.Integrational.Tests.Departments;
using DirectoryService.Integrational.Tests.Locations;
using DirectoryService.WebApi.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests;

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
    private async Task Update_Department_Levels_Success()
    {
        (Guid aId, Guid bId, Guid cId, Guid dId, Guid eId) departments =
            await CreateDepartmentFamily();

        Result<Guid> moving = await _departmentsTests.MoveDepartment(
            departments.aId,
            departments.dId
        );
        Assert.True(moving.IsSuccess);

        Department changedResult = await _departmentsTests.GetDepartment(departments.dId);
        Assert.Equal("department-a.department-d", changedResult.Path.Value);

        Department changedChildResult = await _departmentsTests.GetDepartment(departments.eId);
        Assert.Equal("department-a.department-d.department-e", changedChildResult.Path.Value);
    }

    [Fact]
    private async Task Update_Department_Level_To_Child_Department()
    {
        (Guid aId, Guid bId, Guid cId, Guid dId, Guid eId) departments =
            await CreateDepartmentFamily();

        Result<Guid> moving = await _departmentsTests.MoveDepartment(
            departments.eId,
            departments.dId
        );
        Assert.True(moving.IsFailure);
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

    [Fact]
    private async Task Query_Department_Ancestors_Only_Success()
    {
        const string lowestPath = "department-a.department-c.department-d";

        const string sql = """
            SELECT * FROM departments
            WHERE path @> @lowestPath::ltree
            AND path != @lowestPath::ltree
            """;

        await CreateDepartmentFamily();
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        await using ServiceDbContext db = scope.GetService<ServiceDbContext>();
        DbConnection connection = db.Database.GetDbConnection();

        DepartmentPoco[] items = (
            await connection.QueryAsync<DepartmentPoco>(sql, new { lowestPath })
        ).ToArray();
        Assert.NotEmpty(items);
    }

    [Fact]
    private async Task Ensure_Parent_Is_Owning_Child_Success()
    {
        // получаем структуру где:
        // a - корень
        // b и c являются потомками a
        // d является потомком c
        (Guid aId, Guid bId, Guid cId, Guid dId, Guid eId) departments =
            await CreateDepartmentFamily();
        bool a_owns_b = await IsOwningChild(departments.aId, departments.bId);
        bool a_owns_c = await IsOwningChild(departments.aId, departments.cId);
        bool a_owns_d = await IsOwningChild(departments.aId, departments.dId);
        Assert.All([a_owns_b, a_owns_c, a_owns_d], Assert.True);
    }

    [Fact]
    private async Task Ensure_Child_Is_Not_Owning_Parent_Success()
    {
        // получаем структуру где:
        // a - корень
        // b и c являются потомками a
        // d является потомком c
        (Guid aId, Guid bId, Guid cId, Guid dId, Guid eId) departments =
            await CreateDepartmentFamily();
        bool a_owns_b = await IsOwningChild(departments.bId, departments.aId);
        bool a_owns_c = await IsOwningChild(departments.bId, departments.aId);
        bool a_owns_d = await IsOwningChild(departments.cId, departments.bId);
        Assert.All([a_owns_b, a_owns_c, a_owns_d], Assert.False);
    }

    [Fact]
    private async Task Update_Child_Department_ParentId_Success()
    {
        (Guid aId, Guid bId, Guid cId, Guid dId, Guid eId) departments =
            await CreateDepartmentFamily();
        Guid idOfUpdated = await UpdateChildParentId(departments.dId, departments.aId);
        Result<Department> department = await _departmentsTests.GetDepartment(idOfUpdated);
        Assert.True(department.IsSuccess);
        Assert.NotNull(department.Value.Parent);
        Assert.Equal(department.Value.Parent.Value.Value, departments.aId);
    }

    [Fact]
    private async Task Get_Department_Movement_Success()
    {
        (Guid aId, Guid bId, Guid cId, Guid dId, Guid eId) departments =
            await CreateDepartmentFamily();
        Department parent = await _departmentsTests.GetDepartment(departments.aId);
        Department child = await _departmentsTests.GetDepartment(departments.dId);

        Result<DepartmentMovement> movement = await GetDepartmentMovement(
            departments.aId,
            departments.dId
        );
        Assert.True(movement.IsSuccess);

        Assert.Equal(parent.Id, movement.Value.MovingTo.Id);
        Assert.Equal(child.Id, movement.Value.Movable.Id);
    }

    [Fact]
    private async Task Get_Parent_Department_By_Child_Path_Success()
    {
        (Guid aId, Guid bId, Guid cId, Guid dId, Guid eId) departments =
            await CreateDepartmentFamily();
        Department child = await _departmentsTests.GetDepartment(departments.dId);
        Result<Department> currentParentOfChild = await GetParentDeparmentByChildPath(
            child.Path.Value
        );
        Assert.True(currentParentOfChild.IsSuccess);
    }

    private async Task<Result<Department>> GetParentDeparmentByChildPath(DepartmentPath path)
    {
        return await GetParentDeparmentByChildPath(path.Value);
    }

    private async Task<Result<Department>> GetParentDeparmentByChildPath(string childPath)
    {
        const string sql = """
            SELECT 
                id as Id, 
                "Parent" as ParentId,
                identifier as Identifier,
                name as Name,
                created_at as CreatedAt,
                deleted_at as DeletedAt,
                updated_at as UpdatedAt,
                attachments as Attachments,
                path as Path,
                "Depth" as Depth,
                childrens_count as ChildrensCount
            FROM departments
            WHERE path @> @childPath::ltree AND deleted_at IS NULL
            ORDER BY depth DESC
            LIMIT 1 OFFSET 1
            """;

        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        await using ServiceDbContext db = scope.GetService<ServiceDbContext>();
        DbConnection connection = db.Database.GetDbConnection();
        DepartmentPoco? poco = await connection.QueryFirstOrDefaultAsync<DepartmentPoco>(
            sql,
            new { childPath }
        );

        if (poco == null)
            return Error.NotFoundError("Не найдено родительское подразделение.");
        return poco.ToDepartment();
    }

    private async Task<Guid> UpdateChildParentId(Guid childId, Guid parentId)
    {
        const string sql = """
            UPDATE departments SET "Parent" = @parentId WHERE id = @childId;
            """;

        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        await using ServiceDbContext db = scope.GetService<ServiceDbContext>();
        DbConnection connection = db.Database.GetDbConnection();

        await connection.ExecuteAsync(sql, new { parentId, childId });
        return childId;
    }

    private async Task<Result<DepartmentMovement>> GetDepartmentMovement(
        Guid parentId,
        Guid childId
    )
    {
        const string sql = """
            WITH ancestor_department_info AS
                (SELECT 
                     id,
                     identifier,
                     name,
                     path,
                     "Depth",
                     "Parent",
                     childrens_count,
                     created_at,
                     deleted_at,
                     updated_at,
                     attachments
                 FROM departments 
                 WHERE id = @parentId),
            descendant_department_info AS
                (SELECT
                     id,
                     identifier,
                     name,
                     path,
                     "Depth",
                     "Parent",
                     childrens_count,
                     created_at,
                     deleted_at,
                     updated_at,
                     attachments 
                 FROM departments 
                 WHERE id = @childId)
            SELECT
                ancestor_department_info.path::ltree @> descendant_department_info.path::ltree as is_ancestor_owning,
                ancestor_department_info.id as ancestor_id,
                ancestor_department_info.identifier as ancestor_identifier,
                ancestor_department_info.name as ancestor_name,
                ancestor_department_info."Depth" as ancestor_depth,
                ancestor_department_info.path as ancestor_path,
                ancestor_department_info."Parent" as ancestor_parent_id,
                ancestor_department_info.childrens_count as ancestor_childrens_count,
                ancestor_department_info.created_at as ancestor_created_at,
                ancestor_department_info.deleted_at as ancestor_deleted_at,
                ancestor_department_info.updated_at as ancestor_updated_at,
                ancestor_department_info.attachments as ancestor_attachments,
                
                descendant_department_info.id as descendant_id,
                descendant_department_info.identifier as descendant_identifier,
                descendant_department_info.name as descendant_name,
                descendant_department_info."Depth" as descendant_depth,
                descendant_department_info.path as descendant_path,
                descendant_department_info."Parent" as descendant_parent_id,
                descendant_department_info.childrens_count as descendant_childrens_count,
                descendant_department_info.created_at as descendant_created_at,
                descendant_department_info.deleted_at as descendant_deleted_at,
                descendant_department_info.updated_at as descendant_updated_at,
                descendant_department_info.attachments as descendant_attachments
            FROM ancestor_department_info
                CROSS JOIN descendant_department_info;
            """;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        await using ServiceDbContext db = scope.GetService<ServiceDbContext>();
        DbConnection connection = db.Database.GetDbConnection();

        DepartmentMovementPoco[] movement = (
            await connection.QueryAsync<DepartmentMovementPoco>(sql, new { parentId, childId })
        ).ToArray();

        if (movement.Length == 0)
            return Error.NotFoundError(
                $"Не удается найти информацию для смены подразделения {childId} к {parentId}"
            );

        return movement[0].ToDomainObject();
    }

    private async Task<bool> IsOwningChild(Guid parentId, Guid childId)
    {
        // запрос, который позволяет проверить является ли подразделение, которому мы меняем родителя - нижним по уровню.
        // дополнительно, запрос позволяет вытянуть данные о родительском подразделении (без связей), для дальнейшей работы с ним.
        const string sql = """
            WITH ancestor_department_info AS
                (SELECT id, name, path FROM departments WHERE id = @ancestorId AND deleted_at IS NULL),
            descendant_department_info AS
                (SELECT id, name, path FROM departments WHERE id = @descendantId AND deleted_at IS NULL)
            SELECT
                ancestor_department_info.path::ltree @> descendant_department_info.path::ltree as is_ancestor_owning,
                ancestor_department_info.path as ancestor_path,
                descendant_department_info.path as descendant_path
            FROM ancestor_department_info
                CROSS JOIN descendant_department_info; 
            """;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        await using ServiceDbContext db = scope.GetService<ServiceDbContext>();
        DbConnection connection = db.Database.GetDbConnection();

        DepartmentMovementPoco[] movement = (
            await connection.QueryAsync<DepartmentMovementPoco>(
                sql,
                new { ancestorId = parentId, descendantId = childId }
            )
        ).ToArray();
        return movement.Length == 1 && movement[0].is_ancestor_owning;
    }

    private async Task RefreshDepartmentChildPaths(Department department, DepartmentPath oldPath)
    {
        string newPath = department.Path.Value;
        string oldPathString = oldPath.Value;
        Guid departmentId = department.Id.Value;
        const string sql = """
            UPDATE departments
            SET path = @newPath::ltree || subpath(path::ltree, nlevel(@oldPath::ltree))
            WHERE path <@ @newPath::ltree
            AND id != @departmentId
            """;
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        await using ServiceDbContext db = scope.GetService<ServiceDbContext>();
        DbConnection connection = db.Database.GetDbConnection();
        await connection.ExecuteAsync(
            sql,
            new
            {
                newPath,
                oldPath = oldPathString,
                departmentId,
            }
        );
    }

    private async Task<(Guid aId, Guid bId, Guid cId, Guid dId, Guid eId)> CreateDepartmentFamily()
    {
        Guid locationId = await _locationsTests.CreateNewLocation(
            "Test Location",
            "Test/Location",
            ["Some", "Big", "City"]
        );

        Guid aId = await _departmentsTests.CreateNewDepartment(
            "Department A",
            "department-a",
            [locationId]
        );
        Guid bId = await _departmentsTests.CreateNewDepartment(
            "Department B",
            "department-b",
            [locationId],
            aId
        );
        Guid cId = await _departmentsTests.CreateNewDepartment(
            "Department C",
            "department-c",
            [locationId],
            aId
        );
        Guid dId = await _departmentsTests.CreateNewDepartment(
            "Department D",
            "department-d",
            [locationId],
            cId
        );
        Guid eId = await _departmentsTests.CreateNewDepartment(
            "Department E",
            "department-e",
            [locationId],
            dId
        );
        return (aId, bId, cId, dId, eId);
    }
}
