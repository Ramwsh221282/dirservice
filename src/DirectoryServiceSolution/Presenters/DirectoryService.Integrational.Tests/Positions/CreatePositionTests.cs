using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Integrational.Tests.Departments;
using DirectoryService.Integrational.Tests.Locations;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests.Positions;

public sealed class CreatePositionTests : IClassFixture<TestApplicationFactory>, IAsyncLifetime
{
    private readonly TestApplicationFactory _factory;
    private readonly DepartmentsTestsHelper _departments;
    private readonly LocationsTestsHelper _locations;
    private readonly PositionsTestsHelper _positions;

    public CreatePositionTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _departments = new DepartmentsTestsHelper(factory);
        _locations = new LocationsTestsHelper(factory);
        _positions = new PositionsTestsHelper(factory);
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
    private async Task Create_Position_Empty_Name_Failure()
    {
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";

        Result<Guid> createLocationFirst =
            await _locations.CreateNewLocation("Test Location First", "Test/Location", ["Ленинградская область", "г. Всеволожск", "улица Ленина", "д. 1"]);
        Result<Guid> createLocationSecond =
            await _locations.CreateNewLocation("Test Location Second", "Test/Location", ["Московская область", "г. Химки", "проспект Мира", "д. 2"]);

        Assert.True(createLocationFirst.IsSuccess);
        Assert.True(createLocationSecond.IsSuccess);
        Guid[] createdLocationIds = [createLocationFirst, createLocationSecond];


        Result<Guid> createDepartment =
            await _departments.CreateNewDepartment(expectedDepartmentName, expectedDepartmentIdentifier,
                createdLocationIds);
        Assert.True(createDepartment.IsSuccess);

        Result<Guid> childDepartment =
            await _departments.CreateNewDepartment("Child Dep", "child-dep", createdLocationIds, createDepartment);
        Assert.True(childDepartment.IsSuccess);

        Guid[] createdDepartmentIds = [createDepartment, childDepartment];

        Result<Guid> createPosition =
            await _positions.CreateNewPosition(" ", "Test Position Description", createdDepartmentIds);

        Assert.True(createPosition.IsFailure);
    }

    [Fact]
    private async Task Create_Position_Description_Failure()
    {
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";

        Result<Guid> createLocationFirst =
            await _locations.CreateNewLocation("Test Location First", "Test/Location", ["Ленинградская область", "г. Всеволожск", "улица Ленина", "д. 1"]);
        Result<Guid> createLocationSecond =
            await _locations.CreateNewLocation("Test Location Second", "Test/Location", ["Московская область", "г. Химки", "проспект Мира", "д. 2"]);

        Assert.True(createLocationFirst.IsSuccess);
        Assert.True(createLocationSecond.IsSuccess);
        Guid[] createdLocationIds = [createLocationFirst, createLocationSecond];


        Result<Guid> createDepartment =
            await _departments.CreateNewDepartment(expectedDepartmentName, expectedDepartmentIdentifier,
                createdLocationIds);
        Assert.True(createDepartment.IsSuccess);

        Result<Guid> childDepartment =
            await _departments.CreateNewDepartment("Child Dep", "child-dep", createdLocationIds, createDepartment);
        Assert.True(childDepartment.IsSuccess);

        Guid[] createdDepartmentIds = [createDepartment, childDepartment];

        Result<Guid> createPosition =
            await _positions.CreateNewPosition("Test Position Name", "    ", createdDepartmentIds);

        Assert.True(createPosition.IsFailure);
    }

    [Fact]
    private async Task Create_Position_Departments_NotExist_Failure()
    {
        IEnumerable<Guid> departmentIds = [Guid.NewGuid(), Guid.NewGuid()];

        Result<Guid> createPosition =
            await _positions.CreateNewPosition("Test Position Name", "Test Position Description", departmentIds);

        Assert.True(createPosition.IsFailure);
    }

    [Fact]
    private async Task Create_Position_Empty_Department_Identifiers_Failure()
    {
        Result<Guid> createPosition =
            await _positions.CreateNewPosition("Test Position Name", "Test Position Description", []);

        Assert.True(createPosition.IsFailure);
    }

    [Fact]
    private async Task Create_Position_Success()
    {
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";

        Result<Guid> createLocationFirst =
            await _locations.CreateNewLocation("Test Location First", "Test/Location", ["Ленинградская область", "г. Всеволожск", "улица Ленина", "д. 1"]);
        Result<Guid> createLocationSecond =
            await _locations.CreateNewLocation("Test Location Second", "Test/Location", ["Московская область", "г. Химки", "проспект Мира", "д. 2"]);

        Assert.True(createLocationFirst.IsSuccess);
        Assert.True(createLocationSecond.IsSuccess);
        Guid[] createdLocationIds = [createLocationFirst, createLocationSecond];


        Result<Guid> createDepartment =
            await _departments.CreateNewDepartment(expectedDepartmentName, expectedDepartmentIdentifier,
                createdLocationIds);
        Assert.True(createDepartment.IsSuccess);

        Result<Guid> childDepartment =
            await _departments.CreateNewDepartment("Child Dep", "child-dep", createdLocationIds, createDepartment);
        Assert.True(childDepartment.IsSuccess);

        Guid[] createdDepartmentIds = [createDepartment, childDepartment];

        Result<Guid> createPosition =
            await _positions.CreateNewPosition("Test Position Name", "Test Position Description", createdDepartmentIds);

        Assert.True(createPosition.IsSuccess);

        IEnumerable<DepartmentPosition> departmentPositions =
            await _positions.GetDepartmentPositions(createPosition);

        Assert.Equal(createdDepartmentIds.Length, departmentPositions.Count());
        Assert.True(
            departmentPositions.All(dp =>
                createdDepartmentIds.Any(id => id == dp.DepartmentId.Value)
            )
        );
    }

    [Fact]
    private async Task Create_Position_Duplicate_Name_Failure()
    {
        const string expectedDepartmentName = "Test Department";
        const string expectedDepartmentIdentifier = "test-identifier";

        Result<Guid> createLocationFirst =
            await _locations.CreateNewLocation("Test Location First", "Test/Location", ["Ленинградская область", "г. Всеволожск", "улица Ленина", "д. 1"]);
        Result<Guid> createLocationSecond =
            await _locations.CreateNewLocation("Test Location Second", "Test/Location", ["Московская область", "г. Химки", "проспект Мира", "д. 2"]);

        Assert.True(createLocationFirst.IsSuccess);
        Assert.True(createLocationSecond.IsSuccess);
        Guid[] createdLocationIds = [createLocationFirst, createLocationSecond];


        Result<Guid> createDepartment =
            await _departments.CreateNewDepartment(expectedDepartmentName, expectedDepartmentIdentifier,
                createdLocationIds);
        Assert.True(createDepartment.IsSuccess);

        Result<Guid> childDepartment =
            await _departments.CreateNewDepartment("Child Dep", "child-dep", createdLocationIds, createDepartment);
        Assert.True(childDepartment.IsSuccess);

        Guid[] createdDepartmentIds = [createDepartment, childDepartment];

        Result<Guid> createPosition =
            await _positions.CreateNewPosition("Test Position Name", "Test Position Description", createdDepartmentIds);
        Assert.True(createPosition.IsSuccess);

        Result<Guid> createPositionAgain =
            await _positions.CreateNewPosition("Test Position Name", "Test Position Description", createdDepartmentIds);
        Assert.True(createPositionAgain.IsFailure);
    }
}
