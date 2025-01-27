using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.PowerGeneration.Infrastructure.Configuration
{
    public class PowerGenerationSettings
    {
        public required string InputFolderPath { get; set; }
        public required string OutputFolderPath { get; set; }
        public required string ReferenceDataPath { get; set; }
    }
}
