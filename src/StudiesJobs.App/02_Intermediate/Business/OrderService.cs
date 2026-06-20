using Microsoft.Extensions.Logging;
using StudiesJobs.App._02_Intermediate.Interfaces;

namespace StudiesJobs.App._02_Intermediate.Business;

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public Task ProcessPendingOrdersAsync()
    {
        _logger.LogInformation("Executing business logic for pending orders...");
        return Task.CompletedTask;
    }
}
