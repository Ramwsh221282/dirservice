using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using ResultLibrary;

namespace DirectoryService.Infrastructure.PostgreSQL.Seeding;

public sealed class DepartmentsSeeder : ISeeder
{
    private readonly ServiceDbContext _dbContext;
    private readonly Serilog.ILogger _logger;
    private readonly Random _random = new();
    private readonly DepartmentNameUniquesnessStub _nameUniquesnessStub;

    public DepartmentsSeeder(ServiceDbContext dbContext, Serilog.ILogger logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _nameUniquesnessStub = new DepartmentNameUniquesnessStub(dbContext);
    }

    public async Task SeedAsync()
    {
        _logger.Information("Seeding departments...");

        try { }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred seeding departments.");
        }

        _logger.Information("Departments seeded.");
    }

    private async Task SeedData()
    {
        List<Location> locations = await _dbContext.Locations.ToListAsync();

        List<Department> departments = [];

        var seedDepartments = new[]
        {
            // IT и разработка
            ("Отдел разработки", "dev"),
            ("Отдел тестирования", "qa"),
            ("Отдел аналитики", "analytics"),
            ("Отдел инфраструктуры", "infra"),
            ("Отдел DevOps", "devops"),
            ("Отдел мобильной разработки", "mobile"),
            ("Отдел фронтенда", "frontend"),
            ("Отдел бэкенда", "backend"),
            ("Отдел данных", "data"),
            ("Отдел машинного обучения", "ml"),
            ("Отдел искусственного интеллекта", "ai"),
            ("Отдел кибербезопасности", "cybersec"),
            ("Отдел информационной безопасности", "infosec"),
            ("Отдел сетевых технологий", "networking"),
            ("Отдел системного администрирования", "sysadmin"),
            // Бизнес-подразделения
            ("Отдел маркетинга", "marketing"),
            ("Отдел продаж", "sales"),
            ("Отдел закупок", "procurement"),
            ("Отдел логистики", "logistics"),
            ("Отдел цепочек поставок", "supplychain"),
            ("Отдел клиентского опыта", "cx"),
            ("Отдел поддержки клиентов", "support"),
            ("Отдел партнерских отношений", "partnerships"),
            ("Отдел стратегического развития", "strategy"),
            ("Отдел корпоративных коммуникаций", "comms"),
            // Финансы и управление
            ("Финансовый отдел", "finance"),
            ("Бухгалтерия", "accounting"),
            ("Отдел налогового планирования", "tax"),
            ("Отдел внутреннего аудита", "audit"),
            ("Отдел казначейства", "treasury"),
            ("Отдел бюджетирования", "budgeting"),
            ("Отдел управленческого учета", "managementaccounting"),
            // Персонал и культура
            ("HR-отдел", "hr"),
            ("Отдел подбора персонала", "recruitment"),
            ("Отдел обучения и развития", "lnd"),
            ("Отдел компенсаций и льгот", "compben"),
            ("Отдел организационного развития", "od"),
            ("Отдел корпоративной культуры", "culture"),
            ("Отдел кадрового делопроизводства", "personnel"),
            // Юридические и регуляторные
            ("Юридический отдел", "legal"),
            ("Отдел интеллектуальной собственности", "ip"),
            ("Отдел договорной работы", "contracts"),
            ("Отдел регуляторного соответствия", "compliance"),
            ("Отдел корпоративного права", "corplaw"),
            ("Отдел трудового права", "labourlaw"),
            // Операционные и вспомогательные
            ("Отдел документооборота", "docs"),
            ("Отдел администрирования", "admin"),
            ("Отдел закупок оборудования", "equipproc"),
            ("Отдел управления офисами", "facilities"),
            ("Отдел корпоративных путешествий", "travel"),
            ("Отдел внутренних расследований", "investigations"),
            ("Отдел качества", "quality"),
            ("Отдел стандартизации", "standards"),
            ("Отдел экологической устойчивости", "sustainability"),
            ("Отдел корпоративной социальной ответственности", "csr"),
        };

        foreach (var (name, identifier) in seedDepartments)
        {
            Result<DepartmentName> nameResult = DepartmentName.Create(name);
            if (nameResult.IsFailure)
            {
                _logger.Warning(
                    "Skipping department '{Name}' due to invalid name: {Error}",
                    name,
                    nameResult.Error
                );
                continue;
            }

            if (await _nameUniquesnessStub.HasWithName(nameResult))
            {
                _logger.Warning(
                    "Skipping department '{Name}' due to not unique name",
                    nameResult.Value
                );
                continue;
            }

            Result<DepartmentIdentifier> identifierResult = DepartmentIdentifier.Create(identifier);
            if (identifierResult.IsFailure)
            {
                _logger.Warning(
                    "Skipping department '{Name}' due to invalid identifier '{Identifier}': {Error}",
                    name,
                    identifier,
                    identifierResult.Error
                );
                continue;
            }

            // Выбираем случайное количество локаций: от 1 до 3
            int locationCount = _random.Next(1, Math.Min(4, locations.Count + 1));
            var selectedLocations = locations
                .OrderBy(_ => _random.Next())
                .Take(locationCount)
                .ToList();

            var department = Department.CreateNew(
                nameResult.Value,
                identifierResult.Value,
                selectedLocations
            );

            departments.Add(department);
        }

        if (departments.Count == 0)
        {
            _logger.Warning("No valid departments to seed.");
            return;
        }

        _dbContext.Departments.AddRange(departments);
        await _dbContext.SaveChangesAsync();

        _logger.Information("Successfully seeded {Count} departments.", departments.Count);
    }
}
