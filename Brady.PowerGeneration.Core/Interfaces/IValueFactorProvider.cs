using Brady.PowerGeneration.Core.Enums;
using Brady.PowerGeneration.Core.Models;

namespace Brady.PowerGeneration.Core.Interfaces
{
    public interface IValueFactorProvider
    {
        decimal GetValueFactor(GeneratorType generatorType, ReferenceData referenceData);
        decimal CalculateGenerationValue(decimal energy, decimal price, GeneratorType generatorType, ReferenceData referenceData);
    }
}
