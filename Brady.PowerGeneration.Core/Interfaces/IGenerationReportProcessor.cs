using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.PowerGeneration.Core.Interfaces
{
    public interface IGenerationReportProcessor
    {
        Task ProcessReportAsync(string inputFilePath, string outputFilePath);
    }
}
