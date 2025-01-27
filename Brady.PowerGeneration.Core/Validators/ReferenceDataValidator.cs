using Brady.PowerGeneration.Core.Interfaces;
using Brady.PowerGeneration.Core.Models;
using Microsoft.Extensions.Logging;

namespace Brady.PowerGeneration.Core.Validators
{
    public class ReferenceDataValidator : IXmlDataValidator<ReferenceData>
    {
        private readonly ILogger<ReferenceDataValidator> _logger;

        public ReferenceDataValidator(ILogger<ReferenceDataValidator> logger)
        {
            _logger = logger;
        }

        public Task ValidateAsync(ReferenceData data)
        {
            _logger.LogDebug("Starting reference data validation");

            // Validate the overall structure
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Reference data cannot be null");
            }

            if (data.Factors == null)
            {
                throw new InvalidOperationException("Reference data must contain Factors");
            }

            // Validate Value Factors
            ValidateValueFactors(data.Factors.ValueFactor);

            // Validate Emission Factors
            ValidateEmissionFactors(data.Factors.EmissionsFactor);

            _logger.LogInformation("Reference data validation completed successfully");
            return Task.CompletedTask;
        }

        private void ValidateValueFactors(ValueFactor? factors)
        {
            if (factors == null)
            {
                throw new InvalidOperationException("Value factors are missing from reference data");
            }

            // Ensure all required factors are present and valid
            if (factors.High <= 0)
            {
                throw new InvalidOperationException("High value factor must be greater than zero");
            }

            if (factors.Medium <= 0)
            {
                throw new InvalidOperationException("Medium value factor must be greater than zero");
            }

            if (factors.Low <= 0)
            {
                throw new InvalidOperationException("Low value factor must be greater than zero");
            }
        }

        private void ValidateEmissionFactors(EmissionsFactor? factors)
        {
            if (factors == null)
            {
                throw new InvalidOperationException("Emission factors are missing from reference data");
            }

            // Ensure all required factors are present and valid
            if (factors.High <= 0)
            {
                throw new InvalidOperationException("High emission factor must be greater than zero");
            }

            if (factors.Medium <= 0)
            {
                throw new InvalidOperationException("Medium emission factor must be greater than zero");
            }

            if (factors.Low <= 0)
            {
                throw new InvalidOperationException("Low emission factor must be greater than zero");
            }
        }
    }
}
