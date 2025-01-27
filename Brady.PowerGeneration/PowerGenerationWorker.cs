using Brady.PowerGeneration.Core.Interfaces;
using Brady.PowerGeneration.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brady.PowerGeneration.Console
{
    public class PowerGenerationWorker : BackgroundService
    {
        private readonly IFileMonitor _fileMonitor;
        private readonly PowerGenerationSettings _settings;
        private readonly ILogger<PowerGenerationWorker> _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        public PowerGenerationWorker(
            IFileMonitor fileMonitor,
            IOptions<PowerGenerationSettings> settings,
            ILogger<PowerGenerationWorker> logger,
            IHostApplicationLifetime appLifetime)
        {
            _fileMonitor = fileMonitor;
            _settings = settings.Value;
            _logger = logger;
            _appLifetime = appLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting power generation monitoring...");

                // Validate settings
                if (string.IsNullOrEmpty(_settings.InputFolderPath))
                    throw new InvalidOperationException("Input folder path not configured");
                if (string.IsNullOrEmpty(_settings.OutputFolderPath))
                    throw new InvalidOperationException("Output folder path not configured");

                // Start monitoring the input folder
                _fileMonitor.StartMonitoring(_settings.InputFolderPath, _settings.OutputFolderPath, _settings.ReferenceDataPath);

                _logger.LogInformation("Monitoring started. Watching for XML files in: {Path}",_settings.InputFolderPath);

                // Keep the service running until cancellation is requested
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in power generation monitoring");
                // Initiate graceful shutdown
                _appLifetime.StopApplication();
                throw;
            }
            finally
            {
                _fileMonitor.StopMonitoring();
                _logger.LogInformation("Power generation monitoring stopped");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping power generation monitoring...");
            _fileMonitor.StopMonitoring();
            await base.StopAsync(cancellationToken);
        }
    }
}
