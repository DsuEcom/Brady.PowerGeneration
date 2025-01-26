using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.PowerGeneration.Core.Dtos
{
    public class GenerationReportDto
    {
        public List<GeneratorTotal> Totals { get; set; } = new List<GeneratorTotal>();
        public List<MaxEmissionGenerator> MaxEmissionGenerators { get; set; } = new List<MaxEmissionGenerator>();
        public List<ActualHeatRate> ActualHeatRates { get; set; } = new List<ActualHeatRate>();
    }

    public class GeneratorTotal
    {
        public string Name { get; set; }
        public decimal Total { get; set; }
    }

    public class MaxEmissionGenerator
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Emission { get; set; }
    }

    public class ActualHeatRate
    {
        public string Name { get; set; }
        public decimal HeatRate { get; set; }
    }
}
