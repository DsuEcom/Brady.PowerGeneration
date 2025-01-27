using Brady.PowerGeneration.Console;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            // We create a host that will manage our application's lifecycle
            var host = CreateHostBuilder(args).Build();

            // Log application startup
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Power Generation Monitor starting...");

            // Run the application until it's terminated
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            // If we encounter a fatal error, log it and rethrow
            System.Console.WriteLine($"Fatal error occurred: {ex.Message}");
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();
            throw;
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register all our dependencies
                services.AddDependencies(context.Configuration);

                // Register our background service that monitors files
                services.AddHostedService<PowerGenerationWorker>();
            })
            .ConfigureLogging((context, logging) =>
            {
                // Set up logging to console and debug window
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();

                // Use different log levels based on environment
                var isProduction = context.HostingEnvironment.IsProduction();
                logging.SetMinimumLevel(isProduction ? LogLevel.Information : LogLevel.Debug);
            });
}
