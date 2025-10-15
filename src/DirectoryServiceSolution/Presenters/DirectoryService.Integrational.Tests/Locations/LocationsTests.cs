using DirectoryService.Core.LocationsContext;
using ResultLibrary;

namespace DirectoryService.Integrational.Tests.Locations;

public class LocationsTests : IClassFixture<TestApplicationFactory>
{
    private readonly LocationsTestsHelper _helper;

    public LocationsTests(TestApplicationFactory factory)
    {
        _helper = new LocationsTestsHelper(factory);
    }

    [Fact]
    public async Task Create_Location_Name_Duplicate_Failure()
    {
        string name = "Test Location";
        string timeZone = "Big/City";
        IEnumerable<string> additionals =
        [
            "Ленинградская область",
            "г. Санкт-Петербург",
            "проспект Невский",
            "д. 25",
        ];

        Result<Guid> firstLocation = await _helper.CreateNewLocation(name, timeZone, additionals);
        Assert.True(firstLocation.IsSuccess);

        Result<Guid> firstLocationAgain = await _helper.CreateNewLocation(
            name,
            timeZone,
            additionals
        );
        Assert.True(firstLocationAgain.IsFailure);
    }

    [Fact]
    public async Task Create_Location_Success()
    {
        string name = "Test Location";
        string timeZone = "Big/City";
        IEnumerable<string> addressParts =
        [
            "Ленинградская область",
            "г. Санкт-Петербург",
            "проспект Невский",
            "д. 25",
        ];

        Result<Guid> firstLocation = await _helper.CreateNewLocation(name, timeZone, addressParts);
        Assert.True(firstLocation.IsSuccess);

        Result<Location> created = await _helper.GetLocation(firstLocation);
        Assert.True(created.IsSuccess);

        Location location = created.Value;
        Assert.Equal(location.Name.Value, name);
        Assert.Equal(location.TimeZone.Value, timeZone);
        Assert.Contains(location.Address.Parts, p => addressParts.Any(ap => ap.Equals(p.Name)));
    }

    [Fact]
    public async Task Create_Location_Name_Failure()
    {
        string name = "   ";
        string timeZone = "Big/City";
        IEnumerable<string> addressParts =
        [
            "Ленинградская область",
            "г. Санкт-Петербург",
            "проспект Невский",
            "д. 25",
        ];

        Result<Guid> firstLocation = await _helper.CreateNewLocation(name, timeZone, addressParts);
        Assert.True(firstLocation.IsFailure);
    }

    [Fact]
    public async Task Create_Location_TimeZone_Failure()
    {
        string name = "Test Location";
        IEnumerable<string> addressParts =
        [
            "Ленинградская область",
            "г. Санкт-Петербург",
            "проспект Невский",
            "д. 25",
        ];
        string timeZone = "Some Random String";

        Result<Guid> firstLocation = await _helper.CreateNewLocation(name, timeZone, addressParts);
        Assert.True(firstLocation.IsFailure);
    }

    [Fact]
    public async Task Create_Location_Address_Failure()
    {
        string name = "Test Location";
        IEnumerable<string> addressParts = [];
        string timeZone = "Big/City";

        Result<Guid> firstLocation = await _helper.CreateNewLocation(name, timeZone, addressParts);
        Assert.True(firstLocation.IsFailure);
    }
}
