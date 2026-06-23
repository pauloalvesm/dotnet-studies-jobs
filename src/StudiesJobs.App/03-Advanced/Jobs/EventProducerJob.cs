using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudiesJobs.App._03_Advanced.Models;
using StudiesJobs.App._03_Advanced.Queues;

namespace StudiesJobs.App._03_Advanced.Jobs;

public class EventProducerJob : BackgroundService
{
    private readonly InMemoryIntegrationQueue _queue;
    private readonly ILogger<EventProducerJob> _logger;

    public EventProducerJob(InMemoryIntegrationQueue queue, ILogger<EventProducerJob> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int counter = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var newItem = new IntegrationItem(Guid.NewGuid(), $"Event payload number {counter++}");

            _logger.LogInformation("=> PRODUCER: Generating event and sending to internal queue.");
            await _queue.WriteToQueueAsync(newItem);

            await Task.Delay(2000, stoppingToken);
        }
    }
}
