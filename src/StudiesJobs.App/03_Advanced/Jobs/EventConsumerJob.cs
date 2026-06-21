using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudiesJobs.App._03_Advanced.Queues;

namespace StudiesJobs.App._03_Advanced.Jobs;

public class EventConsumerJob : BackgroundService
{
    private readonly InMemoryIntegrationQueue _queue;
    private readonly ILogger<EventConsumerJob> _logger;

    public EventConsumerJob(InMemoryIntegrationQueue queue, ILogger<EventConsumerJob> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Consumer job started and waiting for items...");

        await foreach (var item in _queue.ReadFromQueueAsync(stoppingToken))
        {
            _logger.LogInformation("<= CONSUMER: Processing event ID: {Id} | Content: {Payload}", item.Id, item.Payload);
            await Task.Delay(500, stoppingToken);
        }
    }
}
