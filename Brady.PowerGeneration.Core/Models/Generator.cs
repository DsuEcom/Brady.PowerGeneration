using Brady.PowerGeneration.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Brady.PowerGeneration.Core.Models
{
    [XmlRoot("GenerationReport")]
    public class GenerationReport
    {
        public Wind Wind { get; set; }
        public Gas Gas { get; set; }
        public Coal Coal { get; set; }

        public IEnumerable<Generator> GetAllGenerators()
        {
            var generators = new List<Generator>();
            if (Wind?.WindGenerator != null) generators.AddRange(Wind.WindGenerator);
            if (Gas?.GasGenerator != null) generators.AddRange(Gas.GasGenerator);
            if (Coal?.CoalGenerator != null) generators.AddRange(Coal.CoalGenerator);
            return generators;
        }
    }

    public class Wind
    {
        public List<WindGenerator> WindGenerator { get; set; } = new();
    }

    public class Gas
    {
        public List<GasGenerator> GasGenerator { get; set; } = new();
    }

    public class Coal
    {
        public List<CoalGenerator> CoalGenerator { get; set; } = new();
    }

    public abstract class Generator
    {
        public string Name { get; set; }
        public GenerationData Generation { get; set; }
        public abstract GeneratorType GetGeneratorType();
    }

    public class WindGenerator : Generator
    {
        public string Location { get; set; }
        public override GeneratorType GetGeneratorType() =>
            Location.Equals("Offshore", StringComparison.OrdinalIgnoreCase)
                ? GeneratorType.OffshoreWind
                : GeneratorType.OnshoreWind;
    }

    public class GasGenerator : Generator
    {
        public decimal EmissionsRating { get; set; }
        public override GeneratorType GetGeneratorType() => GeneratorType.Gas;
    }

    public class CoalGenerator : Generator
    {
        public decimal EmissionsRating { get; set; }
        public decimal TotalHeatInput { get; set; }
        public decimal ActualNetGeneration { get; set; }
        public override GeneratorType GetGeneratorType() => GeneratorType.Coal;
    }

    public class GenerationData
    {
        public List<GenerationDay> Day { get; set; } = new();
    }

    public class GenerationDay
    {
        public DateTime Date { get; set; }
        public decimal Energy { get; set; }
        public decimal Price { get; set; }
    }
}
