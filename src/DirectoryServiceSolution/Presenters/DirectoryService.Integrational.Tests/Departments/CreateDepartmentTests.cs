using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Integrational.Tests.Locations;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests.Departments;

public sealed class CreateDepartmentTests : IClassFixture<TestApplicationFactory>, IAsyncLifetime
{
    private readonly TestApplicationFactory _factory;
    private readonly DepartmentsTestsHelper _departmentsHelper;
    private readonly LocationsTestsHelper _locationsHelper;

    public CreateDepartmentTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _departmentsHelper = new DepartmentsTestsHelper(factory);
        _locationsHelper = new LocationsTestsHelper(factory);
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
    private async Task Create_Department_Root_Success()
    {
        const int expectedAttachmentsCount = 0;
        const int expectedLocationsCount = 2;
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";
        const string expectedDepartmentPath = "test-identifier";
        const int expectedDepartmentDepthLevel = 0;

        Result<Guid> firstLocationId = await _locationsHelper.CreateNewLocation(
            "Test Location First",
            "Test/Location",
            ["Ленинградская область", "г. Всеволожск", "улица Ленина", "д. 1"]
        );

        Result<Guid> secondLocationId = await _locationsHelper.CreateNewLocation(
            "Test Location Second",
            "Test/Location",
            ["Московская область", "г. Химки", "проспект Мира", "д. 2"]
        );

        Assert.True(firstLocationId.IsSuccess);
        Assert.True(secondLocationId.IsSuccess);
        IEnumerable<Guid> createdLocationIds = [firstLocationId, secondLocationId];

        Result<Guid> createdDepartment = await _departmentsHelper.CreateNewDepartment(
            expectedDepartmentName,
            expectedDepartmentIdentifier,
            createdLocationIds
        );

        Assert.True(createdDepartment.IsSuccess);

        Result<Department> created = await _departmentsHelper.GetDepartment(createdDepartment);
        Assert.True(created.IsSuccess);

        Department department = created.Value;
        Assert.Equal(expectedDepartmentName, department.Name.Value);
        Assert.Equal(expectedDepartmentIdentifier, department.Identifier.Value);
        Assert.Equal(expectedDepartmentPath, department.Path.Value);
        Assert.Equal(expectedDepartmentDepthLevel, department.Depth.Value);
        Assert.Equal(expectedAttachmentsCount, department.Attachments.Count());
        Assert.Equal(expectedLocationsCount, department.Locations.Count);

        Assert.True(
            department.Locations.All(l => createdLocationIds.Any(cr => cr == l.LocationId.Value))
        );
    }

    [Fact]
    private async Task Create_Child_Department_Success()
    {
        Result<Guid> firstLocationId = await _locationsHelper.CreateNewLocation(
            "Test Location First",
            "Test/Location",
            ["Ленинградская область", "г. Всеволожск", "улица Ленина", "д. 1"]
        );

        Result<Guid> secondLocation = await _locationsHelper.CreateNewLocation(
            "Test Location Second",
            "Test/Location",
            ["Московская область", "г. Химки", "проспект Мира", "д. 2"]
        );


        Assert.True(firstLocationId.IsSuccess);
        Assert.True(secondLocation.IsSuccess);

        IEnumerable<Guid> locationIds = [firstLocationId, secondLocation];

        Result<Guid> createdParentId = await _departmentsHelper.CreateNewDepartment(
            "First Department",
            "first",
            locationIds
        );

        Assert.True(createdParentId.IsSuccess);

        Result<Guid> childDepartmentId = await _departmentsHelper.CreateNewDepartment(
            "Second department",
            "second",
            locationIds,
            createdParentId
        );

        Assert.True(childDepartmentId.IsSuccess);

        Result<Department> created = await _departmentsHelper.GetDepartment(createdParentId);
        Assert.True(created.IsSuccess);
        Department department = created.Value;
        Assert.Equal(1, department.Attachments.Count());
        Assert.Equal(1, department.ChildrensCount.Value);
    }

    [Fact]
    private async Task Create_Child_Department_Twice_Failure()
    {
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";

        Result<Guid> firstLocationId = await _locationsHelper.CreateNewLocation(
            "Test Location First",
            "Test/Location",
            ["Ленинградская область", "г. Всеволожск", "улица Ленина", "д. 1"]
        );

        Result<Guid> secondLocation = await _locationsHelper.CreateNewLocation(
            "Test Location Second",
            "Test/Location",
            ["Московская область", "г. Химки", "проспект Мира", "д. 2"]
        );

        Assert.True(firstLocationId.IsSuccess);
        Assert.True(secondLocation.IsSuccess);

        IEnumerable<Guid> locationIds = [firstLocationId, secondLocation];

        Result<Guid> createDepartment = await _departmentsHelper.CreateNewDepartment(
            expectedDepartmentName,
            expectedDepartmentIdentifier,
            locationIds
        );
        Assert.True(createDepartment.IsSuccess);

        Result<Guid> createChild = await _departmentsHelper.CreateNewDepartment(
            "Child Dep",
            "child-dep",
            locationIds,
            createDepartment
        );
        Assert.True(createChild.IsSuccess);

        Result<Guid> createChildAgain = await _departmentsHelper.CreateNewDepartment(
            "Child Dep",
            "child-dep",
            locationIds,
            createDepartment
        );
        Assert.True(createChildAgain.IsFailure);
    }
}
