using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Integrational.Tests.Departments;
using DirectoryService.Integrational.Tests.Locations;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests;

public sealed class LtreeDepartmentsTests : IClassFixture<RealDatabaseTestApplicationFactory>
{
    private readonly DepartmentsTestsHelper _departmentsTests;
    private readonly LocationsTestsHelper _locationsTests;

    public LtreeDepartmentsTests(RealDatabaseTestApplicationFactory factory)
    {
        _departmentsTests = new DepartmentsTestsHelper(factory);
        _locationsTests = new LocationsTestsHelper(factory);
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
