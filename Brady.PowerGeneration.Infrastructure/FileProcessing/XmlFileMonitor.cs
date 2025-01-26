using Brady.PowerGeneration.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.PowerGeneration.Infrastructure.FileProcessing
{
    public class XmlFileMonitor : IFileMonitor
    {
        private readonly IGenerationReportProcessor _reportProcessor;
        private FileSystemWatcher _watcher;
        private string _outputPath;
        private bool _isProcessing;

        public XmlFileMonitor(IGenerationReportProcessor reportProcessor)
        {
            _reportProcessor = reportProcessor;
        }

        public void StartMonitoring(string inputPath, string outputPath)
        {
            _outputPath = outputPath;

            _watcher = new FileSystemWatcher
            {
                Path = inputPath,
                Filter = "*.xml",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            _watcher.Created += OnFileCreated;
            _watcher.EnableRaisingEvents = true;
        }

        public void StopMonitoring()
        {
            _watcher?.Dispose();
            _watcher = null;
        }

        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (_isProcessing) return;

            try
            {
                _isProcessing = true;
                await _reportProcessor.ProcessReportAsync(e.FullPath, _outputPath);
            }
            catch (Exception ex)
            {
                // Log error appropriately
                Console.WriteLine($"Error processing file: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }
}
