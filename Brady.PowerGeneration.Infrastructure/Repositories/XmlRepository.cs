using Brady.PowerGeneration.Core.Dtos;
using Brady.PowerGeneration.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Brady.PowerGeneration.Infrastructure.Repositories
{
    public class XmlRepository
    {
        public async Task<GenerationReport> LoadReportAsync(string filePath)
        {
            using var reader = new StreamReader(filePath);
            var xmlContent = await reader.ReadToEndAsync();

            var serializer = new XmlSerializer(typeof(GenerationReport));
            using var stringReader = new StringReader(xmlContent);
            return (GenerationReport)serializer.Deserialize(stringReader);
        }

        public async Task SaveOutputAsync(GenerationReportDto output, string filePath)
        {
            var serializer = new XmlSerializer(typeof(GenerationReportDto));
            using var writer = new StreamWriter(filePath);
            serializer.Serialize(writer, output);
        }
    }
}
