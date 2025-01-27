namespace Brady.PowerGeneration.Core.Interfaces
{
    public interface IFileMonitor : IDisposable
    {
        void StartMonitoring(string inputPath, string outputPath, string referencePath);
        void StopMonitoring();
    }
}
