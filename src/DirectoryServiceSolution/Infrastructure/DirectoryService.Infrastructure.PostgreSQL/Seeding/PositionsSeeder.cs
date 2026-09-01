using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.PositionsContext;
using DirectoryService.Core.PositionsContext.ValueObjects;
using DirectoryService.UseCases.Common.Transaction;
using DirectoryService.UseCases.Departments.Contracts;
using DirectoryService.UseCases.Positions.Contracts;
using ResultLibrary;

namespace DirectoryService.Infrastructure.PostgreSQL.Seeding;

public sealed class PositionsSeeder : ISeeder
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IDepartmentPositionsRepository _departmentPositionsRepository;
    private readonly ITransactionSource _transactionSource;
    private readonly Serilog.ILogger _logger;
    private readonly Random _random = new();

    public PositionsSeeder(
        IPositionsRepository positionsRepository,
        IDepartmentsRepository departmentsRepository,
        IDepartmentPositionsRepository departmentPositionsRepository,
        ITransactionSource transactionSource,
        Serilog.ILogger logger)
    {
        _positionsRepository = positionsRepository;
        _departmentsRepository = departmentsRepository;
        _departmentPositionsRepository = departmentPositionsRepository;
        _transactionSource = transactionSource;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.Information("Seeding positions...");
        await using ITransactionScope transaction = await _transactionSource.ReceiveTransaction();

        try
        {
            await SeedData();
            await transaction.CommitChanges(nameof(PositionsSeeder));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Positions seeding exception");
        }

        _logger.Information("Positions seeding complete");
    }

    private async Task SeedData()
    {
        List<Department> allDepartments =
            [.. await _departmentsRepository.Get(new DepartmentSpecification())];

        if (allDepartments.Count == 0)
        {
            _logger.Warning("No departments found. Skipping position seeding.");
            return;
        }

        List<Position> positionsToSeed = [];

        (string, string)[] seedPositions =
        [
            (
                "Руководитель отдела",
                "Руководит работой отдела, координирует задачи и отвечает за результаты команды."
            ),
            (
                "Старший разработчик",
                "Разрабатывает сложные модули системы, участвует в архитектурных решениях."
            ),
            ("Разработчик", "Реализует функциональные требования в коде, пишет unit-тесты."),
            ("Младший разработчик", "Участвует в разработке под наставничеством старших коллег."),
            ("Тестировщик", "Проводит ручное и автоматизированное тестирование ПО."),
            (
                "Автоматизатор тестирования",
                "Разрабатывает и поддерживает фреймворки автоматизированного тестирования."
            ),
            (
                "Аналитик",
                "Собирает и анализирует бизнес-требования, формирует технические задания."
            ),
            ("Бизнес-аналитик", "Выступает связующим звеном между бизнесом и IT."),
            ("Маркетолог", "Разрабатывает и реализует маркетинговые кампании."),
            ("SMM-менеджер", "Ведёт социальные сети компании, взаимодействует с аудиторией."),
            ("Менеджер по продажам", "Продает продукты/услуги, ведёт переговоры с клиентами."),
            (
                "Аккаунт-менеджер",
                "Сопровождает ключевых клиентов, обеспечивает их удовлетворенность."
            ),
            ("Финансовый аналитик", "Анализирует финансовую отчетность, готовит прогнозы."),
            ("Бухгалтер", "Ведёт бухгалтерский и налоговый учет."),
            ("HR-менеджер", "Занимается подбором, адаптацией и развитием персонала."),
            ("Рекрутер", "Ищет и привлекает кандидатов на вакансии."),
            ("Юрист", "Консультирует по правовым вопросам, готовит договоры."),
            (
                "Специалист по поддержке",
                "Отвечает на запросы клиентов, решает технические проблемы."
            ),
            ("Системный администратор", "Обеспечивает стабильную работу ИТ-инфраструктуры."),
            ("DevOps-инженер", "Автоматизирует процессы сборки, тестирования и деплоя."),
            ("Дизайнер", "Создает пользовательские интерфейсы и графические материалы."),
            ("UX-исследователь", "Изучает поведение пользователей для улучшения продукта."),
            ("Data Scientist", "Строит модели машинного обучения на основе данных."),
            ("BI-аналитик", "Создает дашборды и отчеты для принятия решений."),
            ("Логист", "Организует перевозки и хранение товаров."),
            ("Менеджер по закупкам", "Осуществляет закупку оборудования и материалов."),
            ("Комплаенс-менеджер", "Следит за соблюдением законодательства и внутренних политик."),
            ("Специалист по информационной безопасности", "Защищает данные и системы от угроз."),
            ("Администратор офиса", "Обеспечивает работу офисной инфраструктуры."),
            ("Корпоративный тренер", "Проводит обучение сотрудников по ключевым компетенциям."),
        ];

        foreach ((string? nameStr, string? descStr) in seedPositions)
        {
            Result<PositionName> nameResult = PositionName.Create(nameStr);
            if (nameResult.IsFailure)
            {
                _logger.Warning(
                    "Skipping position due to invalid name: {Name}. Error: {Error}",
                    nameStr,
                    nameResult.Error
                );
                continue;
            }

            Result<PositionDescription> descResult = PositionDescription.Create(descStr);
            if (descResult.IsFailure)
            {
                _logger.Warning(
                    "Skipping position '{Name}' due to invalid description. Error: {Error}",
                    nameStr,
                    descResult.Error
                );
                continue;
            }

            PositionNameUniquesness uniquesness = await _positionsRepository.IsUnique(
                nameResult.Value
            );

            // Выбираем случайные подразделения: от 1 до 3
            int deptCount = _random.Next(1, Math.Min(4, allDepartments.Count + 1));
            List<Department> selectedDepartments = [.. allDepartments
                .OrderBy(_ => _random.Next())
                .Take(deptCount)];

            // Создаём должность и связываем с подразделениями
            Result<Position> positionResult = Position.CreateNew(
                nameResult.Value,
                descResult.Value,
                uniquesness,
                selectedDepartments
            );

            if (positionResult.IsFailure)
            {
                _logger.Warning(
                    "Failed to create position '{Name}': {Error}",
                    nameStr,
                    positionResult.Error
                );
                continue;
            }

            positionsToSeed.Add(positionResult.Value);
        }

        if (positionsToSeed.Count == 0)
        {
            _logger.Warning("No valid positions to seed.");
            return;
        }

        foreach (Position position in positionsToSeed)
        {
            await _positionsRepository.Add(position);

            foreach (DepartmentPosition departmentPosition in position.Departments)
            {
                await _departmentPositionsRepository.Add(departmentPosition);
            }
        }

        _logger.Information("Successfully seeded {Count} positions.", positionsToSeed.Count);
    }
}
