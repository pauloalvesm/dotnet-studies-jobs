using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudiesJobs.App._02_Intermediate.Interfaces;

namespace StudiesJobs.App._02_Intermediate;

public class OrderProcessingJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _period = TimeSpan.FromSeconds(10);

    public OrderProcessingJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                await orderService.ProcessPendingOrdersAsync();
            }

            await Task.Delay(_period, stoppingToken);
        }
    }
}
