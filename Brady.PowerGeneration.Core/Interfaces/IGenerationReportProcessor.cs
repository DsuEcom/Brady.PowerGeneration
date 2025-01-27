namespace Brady.PowerGeneration.Core.Interfaces
{
    public interface IGenerationReportProcessor
    {
        Task ProcessReportAsync(string inputFilePath, string outputFilePath, string referenceDataFilePath);
    }
}
