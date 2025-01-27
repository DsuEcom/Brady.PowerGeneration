using System.Xml.Serialization;

namespace Brady.PowerGeneration.Core.Models
{
    /// <summary>
    /// Represents the reference data containing value and emissions factors
    /// This data is loaded from ReferenceData.xml and used in calculations
    /// </summary>
    [XmlRoot("ReferenceData")]
    public class ReferenceData
    {
        [XmlElement("Factors")]
        public Factors? Factors { get; set; }
    }
    
    public class Factors
    {
        [XmlElement("ValueFactor")]
        public ValueFactor? ValueFactor { get; set; }

        [XmlElement("EmissionsFactor")]
        public EmissionsFactor? EmissionsFactor { get; set; }
    }

    public class ValueFactor
    {
        [XmlElement("High")]
        public decimal High { get; set; }

        [XmlElement("Medium")]
        public decimal Medium { get; set; }

        [XmlElement("Low")]
        public decimal Low { get; set; }
    }

    public class EmissionsFactor
    {
        [XmlElement("High")]
        public decimal High { get; set; }

        [XmlElement("Medium")]
        public decimal Medium { get; set; }

        [XmlElement("Low")]
        public decimal Low { get; set; }
    }
}
