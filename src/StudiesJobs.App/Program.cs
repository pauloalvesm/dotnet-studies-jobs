using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudiesJobs.App._01_Basic;

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
                services.AddHostedService<HeartbeatJob>();
            }).Build();

        await host.RunAsync();
    }
}