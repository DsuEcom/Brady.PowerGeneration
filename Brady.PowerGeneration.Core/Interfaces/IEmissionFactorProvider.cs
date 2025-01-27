using Brady.PowerGeneration.Core.Enums;
using Brady.PowerGeneration.Core.Models;

namespace Brady.PowerGeneration.Core.Interfaces
{
    public interface IEmissionFactorProvider
    {
        decimal GetEmissionFactor(GeneratorType generatorType, ReferenceData referenceData);
        decimal CalculateEmissions(decimal energy, decimal emissionsRating, GeneratorType generatorType, ReferenceData referenceData);
    }
}
