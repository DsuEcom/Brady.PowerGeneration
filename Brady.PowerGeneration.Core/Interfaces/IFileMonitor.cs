using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.PowerGeneration.Core.Interfaces
{
    public interface IFileMonitor : IDisposable
    {
        void StartMonitoring(string inputPath, string outputPath);
        void StopMonitoring();
    }
}
