using DirectoryService.Infrastructure.Identity.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResultLibrary;

namespace DirectoryService.Infrastructure.Identity.BackgroundServices;

public sealed class ExpiredAccessTokenCleanupService : BackgroundService
{
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredAccessTokenCleanupService> _logger;

    public ExpiredAccessTokenCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredAccessTokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(StartupDelay, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await ClearExpiredAccessTokens(stoppingToken);

            try
            {
                await Task.Delay(PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ClearExpiredAccessTokens(CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            ISessionsRepository sessionsRepository = scope.ServiceProvider
                .GetRequiredService<ISessionsRepository>();

            Result result = await sessionsRepository.ClearExpiredAccessTokens(stoppingToken);

            if (result.IsFailure)
            {
                _logger.LogError(
                    "Не удалось очистить истёкшие access-токены: {Error}",
                    result.Error
                );
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при очистке истёкших access-токенов.");
        }
    }
}
