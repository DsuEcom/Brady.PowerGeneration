using System.Xml.Serialization;

[XmlRoot("GenerationOutput")]
public class GenerationReportDtoOut
{
    [XmlElement("Totals")]
    public TotalsContainer Totals { get; set; } = new();

    [XmlElement("MaxEmissionGenerators")]
    public MaxEmissionGeneratorsContainer MaxEmissionGenerators { get; set; } = new();

    [XmlElement("ActualHeatRates")]
    public ActualHeatRatesContainer ActualHeatRates { get; set; } = new();
}

public class TotalsContainer
{
    [XmlElement("Generator")]
    public List<GeneratorTotal> Generator { get; set; } = new();
}

public class GeneratorTotal
{
    [XmlElement("Name")]
    public string? Name { get; set; }

    [XmlElement("Total")]
    public decimal? Total { get; set; }
}

public class MaxEmissionGeneratorsContainer
{
    [XmlElement("Day")]
    public List<MaxEmissionGenerator> Day { get; set; } = new();
}

public class MaxEmissionGenerator
{
    [XmlElement("Name")]
    public string? Name { get; set; }

    [XmlElement("Date")]
    public DateTime? Date { get; set; }

    [XmlElement("Emission")]
    public decimal? Emission { get; set; }
}

public class ActualHeatRatesContainer
{
    [XmlElement("ActualHeatRate")]
    public List<ActualHeatRate> ActualHeatRate { get; set; } = new();
}

public class ActualHeatRate
{
    [XmlElement("Name")]
    public string? Name { get; set; }

    [XmlElement("HeatRate")]
    public decimal HeatRate { get; set; }
}