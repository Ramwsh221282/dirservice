using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Integrational.Tests.Locations;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests.Departments;

public sealed class DeleteDepartmentTests : IClassFixture<TestApplicationFactory>, IAsyncLifetime
{
    private readonly TestApplicationFactory _factory;
    private readonly DepartmentsTestsHelper _departments;
    private readonly LocationsTestsHelper _locations;

    public DeleteDepartmentTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _departments = new DepartmentsTestsHelper(factory);
        _locations = new LocationsTestsHelper(factory);
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabase();
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Delete_Department_Success()
    {
        Guid location = await CreateLocation();

        Result<Guid> department = await _departments.CreateNewDepartment(
            "Test Department",
            "test-identifier",
            [location]
        );
        Assert.True(department.IsSuccess);

        Result<Guid> deleting = await _departments.DeleteDepartment(department);
        Assert.True(deleting.IsSuccess);

        Result<Department> deleted = await _departments.GetDepartment(department);
        Assert.True(deleted.IsFailure);
    }

    [Fact]
    public async Task Delete_Department_Archives_Child_Departments()
    {
        Guid location = await CreateLocation();

        Result<Guid> root = await _departments.CreateNewDepartment(
            "Root Department",
            "root",
            [location]
        );
        Assert.True(root.IsSuccess);

        Result<Guid> child = await _departments.CreateNewDepartment(
            "Child Department",
            "child",
            [location],
            root
        );
        Assert.True(child.IsSuccess);

        Result<Guid> grandChild = await _departments.CreateNewDepartment(
            "Grand Child Department",
            "grand-child",
            [location],
            child
        );
        Assert.True(grandChild.IsSuccess);

        Result<Guid> deleting = await _departments.DeleteDepartment(root);
        Assert.True(deleting.IsSuccess);

        Result<Department> deletedChild = await _departments.GetDepartment(child);
        Assert.True(deletedChild.IsFailure);

        Result<Department> deletedGrandChild = await _departments.GetDepartment(grandChild);
        Assert.True(deletedGrandChild.IsFailure);
    }

    [Fact]
    public async Task Delete_Department_Keeps_Unrelated_Departments_Active()
    {
        Guid location = await CreateLocation();

        Result<Guid> target = await _departments.CreateNewDepartment(
            "Target Department",
            "target",
            [location]
        );
        Assert.True(target.IsSuccess);

        Result<Guid> targetChild = await _departments.CreateNewDepartment(
            "Target Child",
            "target-child",
            [location],
            target
        );
        Assert.True(targetChild.IsSuccess);

        Result<Guid> untouched = await _departments.CreateNewDepartment(
            "Untouched Department",
            "untouched",
            [location]
        );
        Assert.True(untouched.IsSuccess);

        Result<Guid> untouchedChild = await _departments.CreateNewDepartment(
            "Untouched Child",
            "untouched-child",
            [location],
            untouched
        );
        Assert.True(untouchedChild.IsSuccess);

        Result<Guid> deleting = await _departments.DeleteDepartment(target);
        Assert.True(deleting.IsSuccess);

        Result<Department> aliveParent = await _departments.GetDepartment(untouched);
        Assert.True(aliveParent.IsSuccess);

        Result<Department> aliveChild = await _departments.GetDepartment(untouchedChild);
        Assert.True(aliveChild.IsSuccess);
        Assert.Equal("untouched.untouched-child", aliveChild.Value.Path.Value);
    }

    [Fact]
    public async Task Delete_Child_Department_Reduces_Parent_Childrens_Count()
    {
        Guid location = await CreateLocation();

        Result<Guid> parent = await _departments.CreateNewDepartment(
            "Parent Department",
            "parent",
            [location]
        );
        Assert.True(parent.IsSuccess);

        Result<Guid> firstChild = await _departments.CreateNewDepartment(
            "First Child",
            "first-child",
            [location],
            parent
        );
        Assert.True(firstChild.IsSuccess);

        Result<Guid> secondChild = await _departments.CreateNewDepartment(
            "Second Child",
            "second-child",
            [location],
            parent
        );
        Assert.True(secondChild.IsSuccess);

        Result<Department> beforeDelete = await _departments.GetDepartment(parent);
        Assert.True(beforeDelete.IsSuccess);
        Assert.Equal(2, beforeDelete.Value.ChildrensCount.Value);

        Result<Guid> deleting = await _departments.DeleteDepartment(firstChild);
        Assert.True(deleting.IsSuccess);

        Result<Department> afterDelete = await _departments.GetDepartment(parent);
        Assert.True(afterDelete.IsSuccess);
        Assert.Equal(1, afterDelete.Value.ChildrensCount.Value);
    }

    [Fact]
    public async Task Delete_Department_Resets_Childrens_Count_Of_Archived_Subtree()
    {
        Guid location = await CreateLocation();

        Result<Guid> root = await _departments.CreateNewDepartment(
            "Root Department",
            "root",
            [location]
        );
        Assert.True(root.IsSuccess);

        Result<Guid> child = await _departments.CreateNewDepartment(
            "Child Department",
            "child",
            [location],
            root
        );
        Assert.True(child.IsSuccess);

        Result<Guid> grandChild = await _departments.CreateNewDepartment(
            "Grand Child Department",
            "grand-child",
            [location],
            child
        );
        Assert.True(grandChild.IsSuccess);

        Result<Guid> deleting = await _departments.DeleteDepartment(root);
        Assert.True(deleting.IsSuccess);

        Assert.Equal(0, await _departments.GetChildrensCountRaw(root));
        Assert.Equal(0, await _departments.GetChildrensCountRaw(child));
        Assert.Equal(0, await _departments.GetChildrensCountRaw(grandChild));
    }

    [Fact]
    public async Task Delete_Not_Existing_Department_Failure()
    {
        Result<Guid> deleting = await _departments.DeleteDepartment(Guid.NewGuid());
        Assert.True(deleting.IsFailure);
    }

    [Fact]
    public async Task Delete_Department_Twice_Failure()
    {
        Guid location = await CreateLocation();

        Result<Guid> department = await _departments.CreateNewDepartment(
            "Test Department",
            "test-identifier",
            [location]
        );
        Assert.True(department.IsSuccess);

        Result<Guid> firstDeleting = await _departments.DeleteDepartment(department);
        Assert.True(firstDeleting.IsSuccess);

        Result<Guid> secondDeleting = await _departments.DeleteDepartment(department);
        Assert.True(secondDeleting.IsFailure);
    }

    private async Task<Guid> CreateLocation()
    {
        Result<Guid> location = await _locations.CreateNewLocation(
            "Test Location",
            "Test/Location",
            ["Ленинградская область", "г. Всеволожск", "улица Ленина", "д. 1"]
        );

        Assert.True(location.IsSuccess);
        return location.Value;
    }
}
