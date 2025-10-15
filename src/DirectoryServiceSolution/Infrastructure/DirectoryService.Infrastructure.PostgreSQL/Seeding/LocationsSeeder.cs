using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using ResultLibrary;

namespace DirectoryService.Infrastructure.PostgreSQL.Seeding;

public sealed class LocationsSeeder : ISeeder
{
    private readonly ServiceDbContext _context;
    private readonly Serilog.ILogger _logger;

    public LocationsSeeder(ServiceDbContext context, Serilog.ILogger logger)
    {
        _context = context;
        _logger = logger.ForContext<LocationsSeeder>();
    }

    public async Task SeedAsync()
    {
        _logger.Information("Seeding locations...");

        try
        {
            await SeedData();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Seeding locations failed.");
        }

        _logger.Information("Locations seeded");
    }

    private async Task SeedData()
    {
        List<Location> locationsToSeed = [];
        LocationNameUniquesnessStub stub = new LocationNameUniquesnessStub(_context);

        var seedData = new[]
        {
            new
            {
                Name = "Главный офис",
                AddressParts = new[]
                {
                    "Краснодарский край",
                    "г. Краснодар",
                    "ул. Пушкина",
                    "д. Колотушкина",
                },
                TimeZone = "Europe/Moscow",
            },
            new
            {
                Name = "Филиал в Санкт-Петербурге",
                AddressParts = new[]
                {
                    "Московская область",
                    "город Москва",
                    "улица Тверская",
                    "дом 13",
                },
                TimeZone = "Europe/Moscow",
            },
            new
            {
                Name = "Офис в Екатеринбурге",
                AddressParts = new[]
                {
                    "Ленинградская область",
                    "г. Санкт-Петербург",
                    "проспект Невский",
                    "д. 25",
                },
                TimeZone = "Asia/Yekaterinburg",
            },
            new
            {
                Name = "Представительство в Новосибирске",
                AddressParts = new[]
                {
                    "Свердловская область",
                    "город Екатеринбург",
                    "бульвар Ленина",
                    "дом 5А",
                },
                TimeZone = "Asia/Novosibirsk",
            },
            new
            {
                Name = "Центр в Казани",
                AddressParts = new[]
                {
                    "Новосибирская область",
                    "г. Новосибирск",
                    "переулок Садовый",
                    "корпус 3",
                },
                TimeZone = "Europe/Moscow",
            },
            new
            {
                Name = "Филиал в Краснодаре",
                AddressParts = new[]
                {
                    "Томская область",
                    "город Томск",
                    "шоссе Московское",
                    "строение 7",
                },
                TimeZone = "Europe/Moscow",
            },
            new
            {
                Name = "Офис в Владивостоке",
                AddressParts = new[]
                {
                    "Московская область",
                    "посёлок Жаворонки",
                    "СНТ «Рассвет»",
                    "д. 12",
                },
                TimeZone = "Asia/Vladivostok",
            },
            new
            {
                Name = "Представительство в Самаре",
                AddressParts = new[]
                {
                    "Ленинградская область",
                    "деревня Марьино",
                    "днп «Зелёный угол»",
                    "дом 5Б",
                },
                TimeZone = "Europe/Samara",
            },
            new
            {
                Name = "Центр в Ростове-на-Дону",
                AddressParts = new[]
                {
                    "Ростовская область",
                    "г. Ростов-на-Дону",
                    "микрорайон 12",
                    "д. 45",
                },
                TimeZone = "Europe/Moscow",
            },
            new
            {
                Name = "Офис в Иркутске",
                AddressParts = new[]
                {
                    "Кемеровская область",
                    "город Кемерово",
                    "мкр. Центральный",
                    "корп. 2",
                },
                TimeZone = "Asia/Irkutsk",
            },
            new
            {
                Name = "Международный офис в Минске",
                AddressParts = new[]
                {
                    "Самарская область",
                    "г. Самара",
                    "аллея Славы",
                    "владение 8",
                },
                TimeZone = "Europe/Minsk",
            },
            new
            {
                Name = "Филиал в Алматы",
                AddressParts = new[]
                {
                    "Калининградская область",
                    "город Калининград",
                    "улица Багратиона",
                    "литера А",
                },
                TimeZone = "Asia/Almaty",
            },
            new
            {
                Name = "Представительство в Киеве",
                AddressParts = new[]
                {
                    "Челябинская область",
                    "г. Челябинск",
                    "пр. Победы",
                    "д. 15В",
                },
                TimeZone = "Europe/Kiev",
            },
            new
            {
                Name = "Офис в Берлине",
                AddressParts = new[]
                {
                    "Воронежская область",
                    "город Воронеж",
                    "ул. Лизюкова",
                    "к. 10Г",
                },
                TimeZone = "Europe/Berlin",
            },
        };

        foreach (var item in seedData)
        {
            Result<LocationName> nameResult = LocationName.Create(item.Name);
            if (nameResult.IsFailure)
            {
                _logger.Warning(
                    "Skipping location due to invalid name: {Name}. Error: {Error}",
                    item.Name,
                    nameResult.Error
                );
                continue;
            }

            Result<LocationTimeZone> timeZoneResult = LocationTimeZone.Create(item.TimeZone);
            if (timeZoneResult.IsFailure)
            {
                _logger.Warning(
                    "Skipping location '{Name}' due to invalid time zone: {TimeZone}. Error: {Error}",
                    item.Name,
                    item.TimeZone,
                    timeZoneResult.Error
                );
                continue;
            }

            Result<LocationAddress> addressResult = LocationAddress.Create(item.AddressParts);
            if (addressResult.IsFailure)
            {
                _logger.Warning(
                    "Skipping location '{Name}' due to invalid address. Error: {Error}",
                    item.Name,
                    addressResult.Error
                );
                continue;
            }

            LocationNameUniquesness uniquesness = await stub.IsUnique(nameResult);
            Result<Location> locationResult = Location.CreateNew(
                nameResult.Value,
                addressResult.Value,
                timeZoneResult.Value,
                uniquesness
            );

            if (locationResult.IsFailure)
            {
                _logger.Warning(
                    "Skipping location '{Name}' due to validation error: {Error}",
                    item.Name,
                    locationResult.Error
                );
                continue;
            }

            locationsToSeed.Add(locationResult.Value);
        }

        if (locationsToSeed.Count == 0)
        {
            _logger.Warning("No valid locations to seed.");
            return;
        }

        _context.Locations.AddRange(locationsToSeed);
        await _context.SaveChangesAsync();

        _logger.Information("Successfully seeded {Count} locations.", locationsToSeed.Count);
    }
}
