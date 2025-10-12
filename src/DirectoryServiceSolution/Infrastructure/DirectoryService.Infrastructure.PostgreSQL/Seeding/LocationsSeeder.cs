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
            // Россия
            new
            {
                Name = "Главный офис",
                AddressParts = new[] { "Россия", "Москва", "Тверская ул., 1" },
                TimeZone = "Europe/Moscow",
            },
            new
            {
                Name = "Филиал в Санкт-Петербурге",
                AddressParts = new[] { "Россия", "Санкт-Петербург", "Невский пр., 10" },
                TimeZone = "Europe/Moscow",
            },
            new
            {
                Name = "Офис в Екатеринбурге",
                AddressParts = new[] { "Россия", "Екатеринбург", "ул. Ленина, 50" },
                TimeZone = "Asia/Yekaterinburg",
            },
            new
            {
                Name = "Представительство в Новосибирске",
                AddressParts = new[] { "Россия", "Новосибирск", "Красный проспект, 25" },
                TimeZone = "Asia/Novosibirsk",
            },
            new
            {
                Name = "Центр в Казани",
                AddressParts = new[] { "Россия", "Казань", "ул. Баумана, 12" },
                TimeZone = "Europe/Moscow",
            },
            new
            {
                Name = "Филиал в Краснодаре",
                AddressParts = new[] { "Россия", "Краснодар", "ул. Красная, 30" },
                TimeZone = "Europe/Moscow",
            },
            new
            {
                Name = "Офис в Владивостоке",
                AddressParts = new[] { "Россия", "Владивосток", "ул. Светланская, 45" },
                TimeZone = "Asia/Vladivostok",
            },
            new
            {
                Name = "Представительство в Самаре",
                AddressParts = new[] { "Россия", "Самара", "ул. Ленинградская, 8" },
                TimeZone = "Europe/Samara",
            },
            new
            {
                Name = "Центр в Ростове-на-Дону",
                AddressParts = new[] { "Россия", "Ростов-на-Дону", "Большая Садовая ул., 43" },
                TimeZone = "Europe/Moscow",
            },
            new
            {
                Name = "Офис в Иркутске",
                AddressParts = new[] { "Россия", "Иркутск", "ул. Карла Маркса, 15" },
                TimeZone = "Asia/Irkutsk",
            },
            // Ближнее зарубежье
            new
            {
                Name = "Международный офис в Минске",
                AddressParts = new[] { "Беларусь", "Минск", "пр. Независимости, 95" },
                TimeZone = "Europe/Minsk",
            },
            new
            {
                Name = "Филиал в Алматы",
                AddressParts = new[] { "Казахстан", "Алматы", "пр. Абая, 77" },
                TimeZone = "Asia/Almaty",
            },
            new
            {
                Name = "Представительство в Киеве",
                AddressParts = new[] { "Украина", "Киев", "ул. Хрещатик, 1" },
                TimeZone = "Europe/Kiev",
            },
            // Дальнее зарубежье
            new
            {
                Name = "Офис в Берлине",
                AddressParts = new[] { "Германия", "Берлин", "Unter den Linden, 77" },
                TimeZone = "Europe/Berlin",
            },
            new
            {
                Name = "Центр в Дубае",
                AddressParts = new[] { "ОАЭ", "Дубай", "Sheikh Zayed Road, 123" },
                TimeZone = "Asia/Dubai",
            },
            new
            {
                Name = "Представительство в Нью-Йорке",
                AddressParts = new[] { "США", "Нью-Йорк", "5th Avenue, 350" },
                TimeZone = "America/New_York",
            },
            new
            {
                Name = "Офис в Токио",
                AddressParts = new[] { "Япония", "Токио", "Shibuya, 2-1-1" },
                TimeZone = "Asia/Tokyo",
            },
            new
            {
                Name = "Филиал в Лондоне",
                AddressParts = new[] { "Великобритания", "Лондон", "Oxford Street, 100" },
                TimeZone = "Europe/London",
            },
            new
            {
                Name = "Центр в Сингапуре",
                AddressParts = new[] { "Сингапур", "Сингапур", "Orchard Road, 400" },
                TimeZone = "Asia/Singapore",
            },
            new
            {
                Name = "Представительство в Париже",
                AddressParts = new[] { "Франция", "Париж", "Avenue des Champs-Élysées, 50" },
                TimeZone = "Europe/Paris",
            },
            // === Россия (дополнительно) ===
            new
            {
                Name = "Офис в Воронеже",
                AddressParts = new[] { "Россия", "Воронеж", "просп. Революции, 30" },
                TimeZone = "Europe/Moscow",
            },
            new
            {
                Name = "Филиал в Красноярске",
                AddressParts = new[] { "Россия", "Красноярск", "ул. Ленина, 115" },
                TimeZone = "Asia/Krasnoyarsk",
            },
            new
            {
                Name = "Представительство в Перми",
                AddressParts = new[] { "Россия", "Пермь", "ул. Ленина, 52" },
                TimeZone = "Asia/Yekaterinburg",
            },
            new
            {
                Name = "Центр в Волгограде",
                AddressParts = new[] { "Россия", "Волгоград", "просп. Ленина, 10" },
                TimeZone = "Europe/Volgograd",
            },
            new
            {
                Name = "Офис в Саратове",
                AddressParts = new[] { "Россия", "Саратов", "ул. Московская, 20" },
                TimeZone = "Europe/Saratov",
            },
            new
            {
                Name = "Филиал в Тюмени",
                AddressParts = new[] { "Россия", "Тюмень", "ул. Республики, 80" },
                TimeZone = "Asia/Yekaterinburg",
            },
            new
            {
                Name = "Представительство в Омске",
                AddressParts = new[] { "Россия", "Омск", "ул. Ленина, 23" },
                TimeZone = "Asia/Omsk",
            },
            new
            {
                Name = "Центр в Уфе",
                AddressParts = new[] { "Россия", "Уфа", "ул. Ленина, 14" },
                TimeZone = "Asia/Yekaterinburg",
            },
            new
            {
                Name = "Офис в Калининграде",
                AddressParts = new[] { "Россия", "Калининград", "ул. Дм. Донского, 1" },
                TimeZone = "Europe/Kaliningrad",
            },
            new
            {
                Name = "Филиал в Мурманске",
                AddressParts = new[] { "Россия", "Мурманск", "пр. Ленина, 50" },
                TimeZone = "Europe/Moscow",
            },
            // === СНГ и соседние страны ===
            new
            {
                Name = "Офис в Бишкеке",
                AddressParts = new[] { "Киргизия", "Бишкек", "пр. Чуй, 100" },
                TimeZone = "Asia/Bishkek",
            },
            new
            {
                Name = "Представительство в Ереване",
                AddressParts = new[] { "Армения", "Ереван", "просп. Мясникяна, 20" },
                TimeZone = "Asia/Yerevan",
            },
            new
            {
                Name = "Центр в Тбилиси",
                AddressParts = new[] { "Грузия", "Тбилиси", "Руставели просп., 15" },
                TimeZone = "Asia/Tbilisi",
            },
            new
            {
                Name = "Филиал в Душанбе",
                AddressParts = new[] { "Таджикистан", "Душанбе", "пр. Рудаки, 30" },
                TimeZone = "Asia/Dushanbe",
            },
            new
            {
                Name = "Офис в Ашхабаде",
                AddressParts = new[] { "Туркменистан", "Ашхабад", "ул. Арчабиль, 50" },
                TimeZone = "Asia/Ashgabat",
            },
            new
            {
                Name = "Представительство в Баку",
                AddressParts = new[] { "Азербайджан", "Баку", "ул. Низами, 77" },
                TimeZone = "Asia/Baku",
            },
            // === Европа ===
            new
            {
                Name = "Офис в Амстердаме",
                AddressParts = new[] { "Нидерланды", "Амстердам", "Damrak, 20" },
                TimeZone = "Europe/Amsterdam",
            },
            new
            {
                Name = "Центр в Вене",
                AddressParts = new[] { "Австрия", "Вена", "Stephansplatz, 3" },
                TimeZone = "Europe/Vienna",
            },
            new
            {
                Name = "Филиал в Риме",
                AddressParts = new[] { "Италия", "Рим", "Via del Corso, 100" },
                TimeZone = "Europe/Rome",
            },
            new
            {
                Name = "Представительство в Мадриде",
                AddressParts = new[] { "Испания", "Мадрид", "Gran Vía, 25" },
                TimeZone = "Europe/Madrid",
            },
            new
            {
                Name = "Офис в Стокгольме",
                AddressParts = new[] { "Швеция", "Стокгольм", "Drottninggatan, 30" },
                TimeZone = "Europe/Stockholm",
            },
            new
            {
                Name = "Центр в Хельсинки",
                AddressParts = new[] { "Финляндия", "Хельсинки", "Esplanadi, 10" },
                TimeZone = "Europe/Helsinki",
            },
            new
            {
                Name = "Филиал в Варшаве",
                AddressParts = new[] { "Польша", "Варшава", "Nowy Świat, 45" },
                TimeZone = "Europe/Warsaw",
            },
            new
            {
                Name = "Представительство в Праге",
                AddressParts = new[] { "Чехия", "Прага", "Wenceslas Square, 15" },
                TimeZone = "Europe/Prague",
            },
            // === Азия и Океания ===
            new
            {
                Name = "Офис в Сеуле",
                AddressParts = new[] { "Южная Корея", "Сеул", "Myeongdong-gil, 12" },
                TimeZone = "Asia/Seoul",
            },
            new
            {
                Name = "Центр в Бангкоке",
                AddressParts = new[] { "Таиланд", "Бангкок", "Sukhumvit Road, 50" },
                TimeZone = "Asia/Bangkok",
            },
            new
            {
                Name = "Филиал в Куала-Лумпуре",
                AddressParts = new[] { "Малайзия", "Куала-Лумпур", "Jalan Bukit Bintang, 88" },
                TimeZone = "Asia/Kuala_Lumpur",
            },
            new
            {
                Name = "Представительство в Джакарте",
                AddressParts = new[] { "Индонезия", "Джакарта", "Jalan Thamrin, 1" },
                TimeZone = "Asia/Jakarta",
            },
            new
            {
                Name = "Офис в Сиднее",
                AddressParts = new[] { "Австралия", "Сидней", "George Street, 200" },
                TimeZone = "Australia/Sydney",
            },
            new
            {
                Name = "Центр в Мельбурне",
                AddressParts = new[] { "Австралия", "Мельбурн", "Flinders Street, 150" },
                TimeZone = "Australia/Melbourne",
            },
            // === Америка ===
            new
            {
                Name = "Филиал в Лос-Анджелесе",
                AddressParts = new[] { "США", "Лос-Анджелес", "Sunset Boulevard, 6000" },
                TimeZone = "America/Los_Angeles",
            },
            new
            {
                Name = "Представительство в Чикаго",
                AddressParts = new[] { "США", "Чикаго", "Michigan Avenue, 875" },
                TimeZone = "America/Chicago",
            },
            new
            {
                Name = "Офис в Торонто",
                AddressParts = new[] { "Канада", "Торонто", "Yonge Street, 100" },
                TimeZone = "America/Toronto",
            },
            new
            {
                Name = "Центр в Мехико",
                AddressParts = new[] { "Мексика", "Мехико", "Paseo de la Reforma, 222" },
                TimeZone = "America/Mexico_City",
            },
            new
            {
                Name = "Филиал в Сан-Паулу",
                AddressParts = new[] { "Бразилия", "Сан-Паулу", "Avenida Paulista, 1000" },
                TimeZone = "America/Sao_Paulo",
            },
            new
            {
                Name = "Представительство в Буэнос-Айресе",
                AddressParts = new[] { "Аргентина", "Буэнос-Айрес", "Avenida 9 de Julio, 500" },
                TimeZone = "America/Argentina/Buenos_Aires",
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
