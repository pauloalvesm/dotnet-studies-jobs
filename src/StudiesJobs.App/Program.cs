using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudiesJobs.App._01_Basic;
using StudiesJobs.App._02_Intermediate;
using StudiesJobs.App._02_Intermediate.Business;
using StudiesJobs.App._02_Intermediate.Interfaces;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("********** Jobs - Demo **********");

        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = "[HH:mm:ss] ";
                });
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddScoped<IOrderService, OrderService>();

                services.AddHostedService<HeartbeatJob>();
                services.AddHostedService<OrderProcessingJob>();
            }).Build();

        await host.RunAsync();
    }
}