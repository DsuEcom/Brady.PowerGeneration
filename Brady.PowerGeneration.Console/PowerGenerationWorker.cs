using Brady.PowerGeneration.Core.Interfaces;
using Brady.PowerGeneration.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.PowerGeneration.Console
{
    public class PowerGenerationWorker : BackgroundService
    {
        private readonly IFileMonitor _fileMonitor;
        private readonly PowerGenerationSettings _settings;
        private readonly ILogger<PowerGenerationWorker> _logger;

        public PowerGenerationWorker(
            IFileMonitor fileMonitor,
            IOptions<PowerGenerationSettings> settings,
            ILogger<PowerGenerationWorker> logger)
        {
            _fileMonitor = fileMonitor;
            _settings = settings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting power generation monitoring...");

                _fileMonitor.StartMonitoring(
                    _settings.InputFolderPath,
                    _settings.OutputFolderPath);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in power generation monitoring");
                throw;
            }
            finally
            {
                _fileMonitor.StopMonitoring();
            }
        }
    }
}
