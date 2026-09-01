using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Integrational.Tests.Departments;
using DirectoryService.Integrational.Tests.Locations;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests;

public sealed class LtreeDepartmentsTests : IClassFixture<TestApplicationFactory>, IAsyncLifetime
{
    private readonly TestApplicationFactory _factory;
    private readonly DepartmentsTestsHelper _departmentsTests;
    private readonly LocationsTestsHelper _locationsTests;

    public LtreeDepartmentsTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _departmentsTests = new DepartmentsTestsHelper(factory);
        _locationsTests = new LocationsTestsHelper(factory);
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
    private async Task Update_Department_Levels_Success()
    {
        (Guid aId, Guid bId, Guid cId, Guid dId, Guid eId) = await CreateDepartmentFamily();

        Result<Guid> moving = await _departmentsTests.MoveDepartment(aId, dId);
        Assert.True(moving.IsSuccess);

        Department changedResult = await _departmentsTests.GetDepartment(dId);
        Assert.Equal("department-a.department-d", changedResult.Path.Value);

        Department changedChildResult = await _departmentsTests.GetDepartment(eId);
        Assert.Equal("department-a.department-d.department-e", changedChildResult.Path.Value);
    }

    [Fact]
    private async Task Update_Department_Level_To_Child_Department()
    {
        (Guid aId, Guid bId, Guid cId, Guid dId, Guid eId) = await CreateDepartmentFamily();

        Result<Guid> moving = await _departmentsTests.MoveDepartment(
            eId,
            dId
        );
        Assert.True(moving.IsFailure);
    }

    private async Task<(Guid aId, Guid bId, Guid cId, Guid dId, Guid eId)> CreateDepartmentFamily()
    {
        Guid locationId = await _locationsTests.CreateNewLocation(
            "Test Location",
            "Test/Location",
            ["Ленинградская область", "г. Всеволожск", "улица Ленина", "д. 1"]
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
