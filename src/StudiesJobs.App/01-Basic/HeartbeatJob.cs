using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StudiesJobs.App._01_Basic;

public class HeartbeatJob : BackgroundService
{
    private readonly ILogger<HeartbeatJob> _logger;
    private readonly TimeSpan _period = TimeSpan.FromSeconds(5);

    public HeartbeatJob(ILogger<HeartbeatJob> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Heartbeat Job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Checking services status at: {Time}", DateTime.Now.ToString("HH:mm:ss"));
                await ExecuteAvailabilityCheckAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing the Heartbeat Job.");
            }

            await Task.Delay(_period, stoppingToken);
        }

        _logger.LogInformation("Heartbeat Job finished.");
    }

    private Task ExecuteAvailabilityCheckAsync()
    {
        return Task.CompletedTask;
    }
}
