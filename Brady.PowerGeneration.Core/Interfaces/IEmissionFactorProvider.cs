using Brady.PowerGeneration.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.PowerGeneration.Core.Interfaces
{
    public interface IEmissionFactorProvider
    {
        decimal GetEmissionFactor(GeneratorType generatorType);
        decimal CalculateEmissions(decimal energy, decimal emissionsRating, GeneratorType generatorType);
    }
}
