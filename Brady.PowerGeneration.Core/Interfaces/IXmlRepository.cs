namespace Brady.PowerGeneration.Core.Interfaces
{/// <summary>
 /// Handles persistence operations for XML-based generation reports
 /// </summary>
    public interface IXmlRepository<T> where T : class
    {
        Task<T> LoadAsync(string filePath);
        Task SaveAsync(T data, string filePath);
    }
}
