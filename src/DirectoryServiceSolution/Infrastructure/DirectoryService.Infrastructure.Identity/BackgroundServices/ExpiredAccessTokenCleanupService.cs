using DirectoryService.Infrastructure.Identity.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DirectoryService.Infrastructure.Identity.BackgroundServices;

public sealed class ExpiredAccessTokenCleanupService : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;

    public ExpiredAccessTokenCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                ISessionsRepository sessionsRepository = scope.ServiceProvider
                    .GetRequiredService<ISessionsRepository>();
                await sessionsRepository.ClearExpiredAccessTokens(stoppingToken);
            }

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
}
