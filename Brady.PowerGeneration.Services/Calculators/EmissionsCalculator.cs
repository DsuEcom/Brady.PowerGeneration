using Brady.PowerGeneration.Core.Enums;
using Brady.PowerGeneration.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.PowerGeneration.Services.Calculators
{
    public class EmissionsCalculator : IEmissionFactorProvider
    {
        private readonly Dictionary<string, decimal> _emissionFactors;

        public EmissionsCalculator()
        {
            _emissionFactors = new Dictionary<string, decimal>
            {
                { "Gas", 0.562m },
                { "Coal", 0.812m }
            };
        }

        public decimal GetEmissionFactor(GeneratorType generatorType)
        {
            if (!_emissionFactors.TryGetValue(generatorType.ToString(), out decimal factor))
            {
                throw new ArgumentException($"Unknown generator type: {generatorType}");
            }
            return factor;
        }

        public decimal CalculateEmissions(decimal energy, decimal emissionsRating, string generatorType)
        {
            var emissionFactor = GetEmissionFactor(generatorType);
            return energy * emissionsRating * emissionFactor;
        }
    }
}
