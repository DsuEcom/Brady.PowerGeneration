using Brady.PowerGeneration.Core.Enums;
using System.Xml.Serialization;

namespace Brady.PowerGeneration.Core.Models
{
    /// <summary>
    /// Represents the root element of the generation report XML document.
    /// Contains collections of different types of power generators.
    /// </summary>
    [XmlRoot("GenerationReport")]
    public class GenerationReport
    {
        // Note that we don't initialize these properties with new instances
        // because the XML serializer will handle instantiation
        [XmlElement("Wind")]
        public Wind? Wind { get; set; }

        [XmlElement("Gas")]
        public Gas? Gas { get; set; }

        [XmlElement("Coal")]
        public Coal? Coal { get; set; }

        /// <summary>
        /// Provides a flattened view of all generators across all types.
        /// This helper method simplifies operations that need to work with all generators.
        /// </summary>
        public IEnumerable<Generator> GetAllGenerators()
        {
            var generators = new List<Generator>();
            // Using null-conditional operator for safer null handling
            if (Wind?.WindGenerator != null) generators.AddRange(Wind.WindGenerator);
            if (Gas?.GasGenerator != null) generators.AddRange(Gas.GasGenerator);
            if (Coal?.CoalGenerator != null) generators.AddRange(Coal.CoalGenerator);
            return generators;
        }
    }

    /// <summary>
    /// Represents a collection of wind generators in the system.
    /// </summary>
    public class Wind
    {
        // The property name matches the XML element name exactly
        [XmlElement("WindGenerator")]
        public List<WindGenerator> WindGenerator { get; set; } = new();
    }

    public class Gas
    {
        [XmlElement("GasGenerator")]
        public List<GasGenerator> GasGenerator { get; set; } = new();
    }

    public class Coal
    {
        [XmlElement("CoalGenerator")]
        public List<CoalGenerator> CoalGenerator { get; set; } = new();
    }

    /// <summary>
    /// Base class defining common properties for all generator types.
    /// Uses XmlInclude attributes to support proper serialization of derived types.
    /// </summary>
    [XmlInclude(typeof(WindGenerator))]
    [XmlInclude(typeof(GasGenerator))]
    [XmlInclude(typeof(CoalGenerator))]
    public abstract class Generator
    {
        [XmlElement("Name")]
        public string Name { get; set; } = string.Empty;

        [XmlElement("Generation")]
        public GenerationData? Generation { get; set; }

        public abstract GeneratorType GetGeneratorType();
    }

    public class WindGenerator : Generator
    {
        [XmlElement("Location")]
        public string Location { get; set; } = string.Empty;

        public override GeneratorType GetGeneratorType() =>
            Location.Equals("Offshore", StringComparison.OrdinalIgnoreCase)
                ? GeneratorType.OffshoreWind
                : GeneratorType.OnshoreWind;
    }

    public class GasGenerator : Generator
    {
        [XmlElement("EmissionsRating")]
        public decimal EmissionsRating { get; set; }

        public override GeneratorType GetGeneratorType() => GeneratorType.Gas;
    }

    public class CoalGenerator : Generator
    {
        [XmlElement("EmissionsRating")]
        public decimal EmissionsRating { get; set; }

        [XmlElement("TotalHeatInput")]
        public decimal TotalHeatInput { get; set; }

        [XmlElement("ActualNetGeneration")]
        public decimal ActualNetGeneration { get; set; }

        public override GeneratorType GetGeneratorType() => GeneratorType.Coal;
    }

    /// <summary>
    /// Represents the generation data for a specific generator,
    /// containing a collection of daily generation records.
    /// </summary>
    public class GenerationData
    {
        [XmlElement("Day")]
        public List<GenerationDay> Day { get; set; } = new();
    }

    /// <summary>
    /// Represents a single day's generation data including
    /// the date, energy produced, and price information.
    /// </summary>
    public class GenerationDay
    {
        [XmlElement("Date")]
        public DateTime Date { get; set; }

        [XmlElement("Energy")]
        public decimal Energy { get; set; }

        [XmlElement("Price")]
        public decimal Price { get; set; }
    }
}
