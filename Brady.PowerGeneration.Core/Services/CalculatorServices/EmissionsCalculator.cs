using Brady.PowerGeneration.Core.Enums;
using Brady.PowerGeneration.Core.Interfaces;
using Brady.PowerGeneration.Core.Models;
using Microsoft.Extensions.Logging;

namespace Brady.PowerGeneration.Core.Services.HelperServices
{
    public class EmissionsCalculator : IEmissionFactorProvider
    {
        private readonly ILogger<EmissionsCalculator> _logger;

        public EmissionsCalculator(ILogger<EmissionsCalculator> logger)
        {
            _logger = logger;
        }

        public decimal GetEmissionFactor(GeneratorType generatorType, ReferenceData referenceData)
        {
            // Ensure we have valid reference data
            if (referenceData?.Factors?.EmissionsFactor == null)
            {
                _logger.LogError("Reference data or emission factors are missing");
                throw new InvalidOperationException("Reference data not properly initialized");
            }

            // Map generator types to their corresponding emission factors
            // Note: Only fossil fuel generators have emission factors
            var factor = generatorType switch
            {
                GeneratorType.Gas => referenceData.Factors.EmissionsFactor.Medium,     // Gas uses Medium factor
                GeneratorType.Coal => referenceData.Factors.EmissionsFactor.High,      // Coal uses High factor
                GeneratorType.OffshoreWind => 0m,    // Wind generators have no emissions
                GeneratorType.OnshoreWind => 0m,     // Wind generators have no emissions
                _ => throw new ArgumentException($"Unsupported generator type: {generatorType}")
            };

            _logger.LogDebug("Retrieved emission factor {Factor} for generator type {Type}",
                factor, generatorType);

            return factor;
        }

        public decimal CalculateEmissions(decimal energy, decimal emissionsRating, GeneratorType generatorType, ReferenceData referenceData)
        {
            // Input validation
            if (energy < 0) throw new ArgumentException("Energy cannot be negative", nameof(energy));
            if (emissionsRating < 0) throw new ArgumentException("Emissions rating cannot be negative", nameof(emissionsRating));

            // Quick return for non-fossil fuel generators
            if (generatorType is GeneratorType.OffshoreWind or GeneratorType.OnshoreWind)
            {
                return 0m;
            }

            // Get the appropriate emission factor for this generator type
            var emissionFactor = GetEmissionFactor(generatorType, referenceData);

            // Calculate total emissions
            var emissions = energy * emissionsRating * emissionFactor;

            _logger.LogDebug("Calculated emissions {Value} for type {Type} (Energy: {Energy}, Rating: {Rating}, Factor: {Factor})",
                emissions, generatorType, energy, emissionsRating, emissionFactor);

            return emissions;
        }
    }
}
