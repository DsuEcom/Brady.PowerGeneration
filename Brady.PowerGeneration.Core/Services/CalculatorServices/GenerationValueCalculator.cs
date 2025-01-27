using Brady.PowerGeneration.Core.Enums;
using Brady.PowerGeneration.Core.Interfaces;
using Brady.PowerGeneration.Core.Models;
using Microsoft.Extensions.Logging;

namespace Brady.PowerGeneration.Core.Services.HelperServices
{
    public class GenerationValueCalculator : IValueFactorProvider
    {
        private readonly ILogger<GenerationValueCalculator> _logger;

        public GenerationValueCalculator(ILogger<GenerationValueCalculator> logger)
        {
            _logger = logger;
        }

        public decimal GetValueFactor(GeneratorType generatorType, ReferenceData referenceData)
        {
            // Ensure we have valid reference data
            if (referenceData?.Factors?.ValueFactor == null)
            {
                _logger.LogError("Reference data or value factors are missing");
                throw new InvalidOperationException("Reference data not properly initialized");
            }

            // Map generator types to their corresponding value factors
            var factor = generatorType switch
            {
                GeneratorType.OffshoreWind => referenceData.Factors.ValueFactor.Low,    // Offshore wind uses Low factor
                GeneratorType.OnshoreWind => referenceData.Factors.ValueFactor.High,    // Onshore wind uses High factor
                GeneratorType.Gas => referenceData.Factors.ValueFactor.Medium,          // Gas uses Medium factor
                GeneratorType.Coal => referenceData.Factors.ValueFactor.Medium,         // Coal uses Medium factor
                _ => throw new ArgumentException($"Unsupported generator type: {generatorType}")
            };

            _logger.LogDebug("Retrieved value factor {Factor} for generator type {Type}",
                factor, generatorType);

            return factor;
        }

        public decimal CalculateGenerationValue(decimal energy, decimal price, GeneratorType generatorType, ReferenceData referenceData)
        {
            // Input validation
            if (energy < 0) throw new ArgumentException("Energy cannot be negative", nameof(energy));
            if (price < 0) throw new ArgumentException("Price cannot be negative", nameof(price));

            // Get the appropriate value factor for this generator type
            var valueFactor = GetValueFactor(generatorType, referenceData);

            // Calculate total generation value
            var generationValue = energy * price * valueFactor;

            _logger.LogDebug(
                "Calculated generation value {Value} for type {Type} " +
                "(Energy: {Energy}, Price: {Price}, Factor: {Factor})",
                generationValue, generatorType, energy, price, valueFactor);

            return generationValue;
        }
    }
}
