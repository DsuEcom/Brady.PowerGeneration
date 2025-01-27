using Brady.PowerGeneration.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Brady.PowerGeneration.Infrastructure.Repositories
{
    /// <summary>
    /// Handles XML file operations for generation reports
    /// Implements thread-safe file reading and writing
    /// </summary>
    /// <summary>
    /// A generic XML repository that can handle serialization and deserialization of any XML-serializable type.
    /// This class provides thread-safe file operations with comprehensive error handling and logging.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize/deserialize. Must be a class that can be XML serialized.</typeparam>
    public class XmlRepository<T> : IXmlRepository<T>, IDisposable where T : class
    {
        private readonly XmlSerializer _serializer;
        private readonly ILogger<XmlRepository<T>> _logger;
        private readonly IXmlDataValidator<T> _validator;

        /// <summary>
        /// Initializes a new instance of the XmlRepository with necessary dependencies.
        /// </summary>
        /// <param name="logger">Logger for recording operations and errors</param>
        /// <param name="validator">Optional validator for the specific type T</param>
        public XmlRepository(ILogger<XmlRepository<T>> logger, IXmlDataValidator<T> validator = null!)
        {
            _logger = logger;
            _validator = validator;

            // Create serializer with proper XML root attribute configuration
            var overrides = new XmlAttributeOverrides();
            var attributes = new XmlAttributes();

            // This ensures the serializer respects the XML root element name from our class
            var rootAttribute = typeof(T).GetCustomAttribute<XmlRootAttribute>();
            if (rootAttribute != null)
            {
                attributes.XmlRoot = rootAttribute;
            }

            overrides.Add(typeof(T), attributes);
            _serializer = new XmlSerializer(typeof(T), overrides);
        }

        /// <summary>
        /// Asynchronously loads and deserializes an XML file into an object of type T.
        /// </summary>
        /// <param name="filePath">The path to the XML file to load</param>
        /// <returns>The deserialized object of type T</returns>
        public async Task<T> LoadAsync(string filePath)
        {
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8);

            try
            {
                // Deserialize using the properly configured serializer
                var result = (T)_serializer.Deserialize(streamReader)!;

                if (result == null)
                {
                    _logger.LogError("Deserialization returned null for type {Type}", typeof(T).Name);
                    throw new InvalidOperationException($"Failed to deserialize XML content into {typeof(T).Name}");
                }

                // Validate if we have a validator
                if (_validator != null)
                {
                    await _validator.ValidateAsync(result);
                }

                return result;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "XML deserialization failed for type {Type}", typeof(T).Name);
                throw new InvalidOperationException(
                    $"Failed to deserialize XML content into {typeof(T).Name}. Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Asynchronously serializes and saves an object to an XML file.
        /// </summary>
        /// <param name="data">The object to serialize</param>
        /// <param name="filePath">The path where the XML file should be saved</param>
        public async Task SaveAsync(T data, string filePath)
        {
            

            try
            {
                _logger.LogInformation("Saving {Type} to {FilePath}", typeof(T).Name, filePath);

                // Ensure the directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Validate before saving if a validator is provided
                if (_validator != null)
                {
                    await _validator.ValidateAsync(data);
                }

                // Use async file operations with proper encoding
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                // Serialize in a separate thread to avoid blocking
                await Task.Run(() => _serializer.Serialize(streamWriter, data));

                _logger.LogInformation("Successfully saved {Type} to {FilePath}", typeof(T).Name, filePath);
            }
            catch (Exception ex) when (LogError(ex, filePath))
            {
                throw;
            }
            
        }

        /// <summary>
        /// Helper method to log errors with appropriate context.
        /// </summary>
        private bool LogError(Exception ex, string filePath)
        {
            var errorMessage = ex switch
            {
                FileNotFoundException => "File not found",
                UnauthorizedAccessException => "Access denied",
                XmlException => "Invalid XML format",
                _ => "Unexpected error"
            };

            _logger.LogError(ex, "{ErrorMessage} processing {Type} at {FilePath}",
                errorMessage, typeof(T).Name, filePath);

            return true; // Always return true to ensure the exception is rethrown
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}

