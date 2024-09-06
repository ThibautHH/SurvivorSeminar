using Microsoft.Extensions.Options;
using SoulDashboard.Options;

namespace SoulDashboard.Services;

public class SoulConnectionSynchronizationService(IServiceProvider serviceProvider,
    IOptionsMonitor<SoulConnectionOptions> optionsMonitor,
    ILogger<SoulConnectionSynchronizationService> logger,
    TimeProvider timeProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool firstRun = true;

        logger.ServiceRunning(nameof(SoulConnectionSynchronizationService));

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(optionsMonitor.CurrentValue.Synchronization.Interval), timeProvider);

        while (firstRun || await timer.WaitForNextTickAsync(stoppingToken))
        {
            firstRun = false;

            using var scope = serviceProvider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<SoulConnectionDataService>()
                .SynchronizeDataAsync(stoppingToken);
        }
    }
}
