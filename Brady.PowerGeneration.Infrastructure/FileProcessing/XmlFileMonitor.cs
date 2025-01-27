using Brady.PowerGeneration.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Brady.PowerGeneration.Infrastructure.FileProcessing
{
    /// <summary>
    /// Monitors a directory for XML files and processes them using the provided strategy
    /// Implements a reliable file watching system with error handling and logging
    /// </summary>
    public class XmlFileMonitor(IGenerationReportProcessor reportProcessor, ILogger<XmlFileMonitor> logger) : IFileMonitor
    {
        private readonly IGenerationReportProcessor _reportProcessor = reportProcessor;
        private readonly ILogger<XmlFileMonitor> _logger = logger;
        private FileSystemWatcher _watcher;
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
        private string _outputPath;
        private string _referenceDataPath;
        private bool _disposed;
        private readonly Dictionary<string, Timer> _processTimers = new();
        private const int FileProcessDelay = 500;

        public void StartMonitoring(string inputPath, string outputPath, string referencePath)
        {
            try
            {
                _logger.LogInformation("Starting file monitoring on {Path}", inputPath);

                ValidateAndCreateDirectories(inputPath, outputPath);

                _outputPath = outputPath;
                _referenceDataPath = referencePath;

                _watcher = new FileSystemWatcher
                {
                    Path = inputPath,
                    Filter = "*.xml",
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite |
                                 NotifyFilters.CreationTime | NotifyFilters.Size
                };

                // Subscribe to all relevant events
                _watcher.Created += OnFileChanged;
                _watcher.Changed += OnFileChanged;
                _watcher.Renamed += OnFileRenamed;
                _watcher.Error += OnWatcherError;

                _watcher.EnableRaisingEvents = true;

                _logger.LogInformation("File monitoring started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start file monitoring");
                throw;
            }
        }

        private void ValidateAndCreateDirectories(string inputPath, string outputPath)
        {
            if (string.IsNullOrEmpty(inputPath))
                throw new ArgumentException("Input path cannot be null or empty", nameof(inputPath));

            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentException("Output path cannot be null or empty", nameof(outputPath));

            if (!Directory.Exists(inputPath))
            {
                Directory.CreateDirectory(inputPath);
                _logger.LogInformation("Created input directory at {Path}", inputPath);
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
                _logger.LogInformation("Created output directory at {Path}", outputPath);
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (IsTemporaryFile(e.FullPath))
                    return;

                string filePath = e.FullPath;
                _logger.LogDebug("File change detected: {File}", e.Name);

                // Cancel existing timer for this file if any
                if (_processTimers.TryGetValue(filePath, out var existingTimer))
                {
                    existingTimer.Dispose();
                    _processTimers.Remove(filePath);
                }

                // Create new timer
                var timer = new Timer(async state => await ProcessFileWithRetry((string)state!),
                    filePath, FileProcessDelay, Timeout.Infinite);

                _processTimers[filePath] = timer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling file change for {File}", e.Name);
            }
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (Path.GetExtension(e.FullPath).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                OnFileChanged(sender, e);
            }
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            _logger.LogError(e.GetException(), "File system watcher error occurred");

            try
            {
                // Attempt to restart the watcher
                _watcher.EnableRaisingEvents = false;
                _watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restart file system watcher");
            }
        }

        private async Task ProcessFileWithRetry(string filePath, int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    await ProcessFile(filePath);
                    return;
                }
                catch (IOException) when (retryCount < maxRetries - 1)
                {
                    retryCount++;
                    _logger.LogWarning("Retry {Count} for file {File}", retryCount, Path.GetFileName(filePath));
                    await Task.Delay(1000 * retryCount); // Exponential backoff
                }
            }
        }

        private async Task ProcessFile(string filePath)
        {
            await _processingSemaphore.WaitAsync();
            try
            {
                if (!IsFileReady(filePath))
                {
                    _logger.LogInformation("File {File} is not ready for processing", Path.GetFileName(filePath));
                    return;
                }

                var fileName = Path.GetFileName(filePath);
                _logger.LogInformation("Processing file: {File}", fileName);

                var outputFileName = Path.GetFileNameWithoutExtension(fileName) + "-Result.xml";
                var outputPath = Path.Combine(_outputPath, outputFileName);

                await _reportProcessor.ProcessReportAsync(filePath, outputPath, _referenceDataPath);
                _logger.LogInformation("Successfully processed file: {File}", fileName);
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }

        private bool IsFileReady(string filePath)
        {
            try
            {
                using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        private bool IsTemporaryFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            return fileName.StartsWith("~") || fileName.StartsWith(".") || fileName.EndsWith(".tmp");
        }

        public void StopMonitoring()
        {
            try
            {
                _logger.LogInformation("Stopping file monitoring");

                if (_watcher != null)
                {
                    _watcher.EnableRaisingEvents = false;
                    _watcher.Created -= OnFileChanged;
                    _watcher.Changed -= OnFileChanged;
                    _watcher.Renamed -= OnFileRenamed;
                    _watcher.Error -= OnWatcherError;
                }

                // Dispose any pending timers
                foreach (var timer in _processTimers.Values)
                {
                    timer.Dispose();
                }
                _processTimers.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping file monitoring");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                StopMonitoring();
                _watcher?.Dispose();
                _processingSemaphore?.Dispose();

                foreach (var timer in _processTimers.Values)
                {
                    timer.Dispose();
                }
                _processTimers.Clear();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}