using DirectoryService.Integrational.Tests.Locations;

namespace DirectoryService.Integrational.Tests.Departments;

public sealed class CreateDepartmentTests : IClassFixture<TestApplicationFactory>
{
    private readonly DepartmentsTestsHelper _departmentsHelper;
    private readonly LocationsTestsHelper _locationsHelper;

    public CreateDepartmentTests(TestApplicationFactory factory)
    {
        _departmentsHelper = new DepartmentsTestsHelper(factory);
        _locationsHelper = new LocationsTestsHelper(factory);
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

        var firstLocationId = await _locationsHelper.CreateNewLocation(
            "Test Location First",
            "Test/Location",
            ["Test", "Location", "First"]
        );

        var secondLocationId = await _locationsHelper.CreateNewLocation(
            "Test Location Second",
            "Test/Location",
            ["Test", "Location", "Second"]
        );

        Assert.True(firstLocationId.IsSuccess);
        Assert.True(secondLocationId.IsSuccess);
        IEnumerable<Guid> createdLocationIds = [firstLocationId, secondLocationId];

        var createdDepartment = await _departmentsHelper.CreateNewDepartment(
            expectedDepartmentName,
            expectedDepartmentIdentifier,
            createdLocationIds
        );

        Assert.True(createdDepartment.IsSuccess);

        var created = await _departmentsHelper.GetDepartment(createdDepartment);
        Assert.True(created.IsSuccess);

        var department = created.Value;
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
        var firstLocationId = await _locationsHelper.CreateNewLocation(
            "Test Location First",
            "Test/Location",
            ["Test", "Location", "First"]
        );

        var secondLocation = await _locationsHelper.CreateNewLocation(
            "Test Location Second",
            "Test/Location",
            ["Test", "Location", "Second"]
        );


        Assert.True(firstLocationId.IsSuccess);
        Assert.True(secondLocation.IsSuccess);

        IEnumerable<Guid> locationIds = [firstLocationId, secondLocation];

        var createdParentId = await _departmentsHelper.CreateNewDepartment(
            "First Department",
            "first",
            locationIds
        );

        Assert.True(createdParentId.IsSuccess);

        var childDepartmentId = await _departmentsHelper.CreateNewDepartment(
            "Second department",
            "second",
            locationIds,
            createdParentId
        );

        Assert.True(childDepartmentId.IsSuccess);

        var created = await _departmentsHelper.GetDepartment(createdParentId);
        Assert.True(created.IsSuccess);
        var department = created.Value;
        Assert.Equal(1, department.Attachments.Count());
        Assert.Equal(1, department.ChildrensCount.Value);
    }

    [Fact]
    private async Task Create_Child_Department_Twice_Failure()
    {
        const int expectedLocationsCount = 2;
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";

        var firstLocationId = await _locationsHelper.CreateNewLocation(
            "Test Location First",
            "Test/Location",
            ["Test", "Location", "First"]
        );

        var secondLocation = await _locationsHelper.CreateNewLocation(
            "Test Location Second",
            "Test/Location",
            ["Test", "Location", "Second"]
        );

        Assert.True(firstLocationId.IsSuccess);
        Assert.True(secondLocation.IsSuccess);

        IEnumerable<Guid> locationIds = [firstLocationId, secondLocation];

        var createDepartment = await _departmentsHelper.CreateNewDepartment(
            expectedDepartmentName,
            expectedDepartmentIdentifier,
            locationIds
        );
        Assert.True(createDepartment.IsSuccess);

        var createChild = await _departmentsHelper.CreateNewDepartment(
            "Child Dep",
            "child-dep",
            locationIds,
            createDepartment
        );
        Assert.True(createChild.IsSuccess);

        var createChildAgain = await _departmentsHelper.CreateNewDepartment(
            "Child Dep",
            "child-dep",
            locationIds,
            createDepartment
        );
        Assert.True(createChildAgain.IsFailure);
    }
}