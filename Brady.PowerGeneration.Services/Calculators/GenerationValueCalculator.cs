using Brady.PowerGeneration.Core.Enums;
using Brady.PowerGeneration.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.PowerGeneration.Services.Calculators
{
    public class GenerationValueCalculator : IValueFactorProvider
    {
        private readonly Dictionary<string, decimal> _valueFactors;

        public GenerationValueCalculator()
        {
            _valueFactors = new Dictionary<string, decimal>
            {
                { "OffshoreWind", 0.265m },
                { "OnshoreWind", 0.946m },
                { "Gas", 0.696m },
                { "Coal", 0.696m }
            };
        }

        public decimal GetValueFactor(GeneratorType generatorType)
        {
            if (!_valueFactors.TryGetValue(generatorType.ToString(), out decimal factor))
            {
                throw new ArgumentException($"Unknown generator type: {generatorType}");
            }
            return factor;
        }
    }
}
