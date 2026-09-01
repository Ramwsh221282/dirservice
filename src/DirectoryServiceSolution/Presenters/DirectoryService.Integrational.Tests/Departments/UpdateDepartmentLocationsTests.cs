using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Integrational.Tests.Locations;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests.Departments;

public sealed class UpdateDepartmentLocationsTests
    : IClassFixture<TestApplicationFactory>,
        IAsyncLifetime
{
    private readonly TestApplicationFactory _factory;
    private readonly DepartmentsTestsHelper _departments;
    private readonly LocationsTestsHelper _locations;

    public UpdateDepartmentLocationsTests(TestApplicationFactory factory)
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
    public async Task Update_Department_Locations_Success()
    {
        Guid initialLocation = await CreateLocation(
            "Initial Location",
            ["Ленинградская область", "г. Всеволожск", "улица Ленина", "д. 1"]
        );

        Guid replacementLocation = await CreateLocation(
            "Replacement Location",
            ["Московская область", "г. Химки", "проспект Мира", "д. 2"]
        );

        Result<Guid> department = await _departments.CreateNewDepartment(
            "Test Department",
            "test-identifier",
            [initialLocation]
        );
        Assert.True(department.IsSuccess);

        Result<Guid> updating = await _departments.UpdateDepartmentLocations(
            department,
            [replacementLocation]
        );
        Assert.True(updating.IsSuccess);

        IEnumerable<DepartmentLocation> locations =
            await _departments.GetDepartmentLocations(department);

        Assert.Single(locations);
        Assert.Equal(replacementLocation, locations.Single().LocationId.Value);
    }

    [Fact]
    public async Task Update_Department_Locations_Not_Existing_Department_Failure()
    {
        Guid location = await CreateLocation(
            "Some Location",
            ["Ленинградская область", "г. Всеволожск", "улица Ленина", "д. 1"]
        );

        Result<Guid> updating = await _departments.UpdateDepartmentLocations(
            Guid.NewGuid(),
            [location]
        );

        Assert.True(updating.IsFailure);
    }

    [Fact]
    public async Task Update_Department_Locations_Not_Existing_Locations_Failure()
    {
        Guid location = await CreateLocation(
            "Some Location",
            ["Ленинградская область", "г. Всеволожск", "улица Ленина", "д. 1"]
        );

        Result<Guid> department = await _departments.CreateNewDepartment(
            "Test Department",
            "test-identifier",
            [location]
        );
        Assert.True(department.IsSuccess);

        Result<Guid> updating = await _departments.UpdateDepartmentLocations(
            department,
            [Guid.NewGuid()]
        );

        Assert.True(updating.IsFailure);
    }

    private async Task<Guid> CreateLocation(string name, IEnumerable<string> addressParts)
    {
        Result<Guid> location = await _locations.CreateNewLocation(
            name,
            "Test/Location",
            addressParts
        );

        Assert.True(location.IsSuccess);
        return location.Value;
    }
}
