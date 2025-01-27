using Brady.PowerGeneration.Core.Models;
using Brady.PowerGeneration.Core.Interfaces;
using Microsoft.Extensions.Logging;


namespace Brady.PowerGeneration.Core.Validators
{
    public class GenerationReportValidator : IXmlDataValidator<GenerationReportDtoIn>
    {
        private readonly ILogger<GenerationReportValidator> _logger;

        public GenerationReportValidator(ILogger<GenerationReportValidator> logger)
        {
            _logger = logger;
        }

        public Task ValidateAsync(GenerationReportDtoIn report)
        {
            _logger.LogDebug("Beginning validation of generation report");

            // Validate overall report structure
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            if (!report.GetAllGenerators().Any())
            {
                throw new InvalidOperationException("Generation report contains no generator data");
            }

            // Validate each generator
            foreach (var generator in report.GetAllGenerators())
            {
                ValidateGenerator(generator);
            }

            _logger.LogInformation("Generation report validation completed successfully");
            return Task.CompletedTask;
        }

        private void ValidateGenerator(Generator generator)
        {
            // Validate generator name
            if (string.IsNullOrEmpty(generator.Name))
            {
                throw new InvalidOperationException("Found generator with missing name");
            }

            // Validate generation data
            if (generator.Generation?.Day == null || !generator.Generation.Day.Any())
            {
                throw new InvalidOperationException($"Generator {generator.Name} has no generation data");
            }

            // Validate each day's data
            foreach (var day in generator.Generation.Day)
            {
                ValidateGenerationDay(generator.Name, day);
            }

            // Validate type-specific requirements
            switch (generator)
            {
                case WindGenerator wind:
                    ValidateWindGenerator(wind);
                    break;
                case GasGenerator gas:
                    ValidateGasGenerator(gas);
                    break;
                case CoalGenerator coal:
                    ValidateCoalGenerator(coal);
                    break;
            }
        }

        private void ValidateGenerationDay(string generatorName, GenerationDay day)
        {
            if (day.Energy < 0)
            {
                throw new InvalidOperationException(
                    $"Generator {generatorName} has invalid negative energy value for {day.Date}");
            }
            if (day.Price < 0)
            {
                throw new InvalidOperationException(
                    $"Generator {generatorName} has invalid negative price value for {day.Date}");
            }
        }

        private void ValidateWindGenerator(WindGenerator generator)
        {
            if (string.IsNullOrEmpty(generator.Location))
            {
                throw new InvalidOperationException(
                    $"Wind generator {generator.Name} has no location specified");
            }
        }

        private void ValidateGasGenerator(GasGenerator generator)
        {
            if (generator.EmissionsRating < 0)
            {
                throw new InvalidOperationException(
                    $"Gas generator {generator.Name} has invalid negative emissions rating");
            }
        }

        private void ValidateCoalGenerator(CoalGenerator generator)
        {
            if (generator.EmissionsRating < 0)
            {
                throw new InvalidOperationException(
                    $"Coal generator {generator.Name} has invalid negative emissions rating");
            }
            if (generator.TotalHeatInput < 0)
            {
                throw new InvalidOperationException(
                    $"Coal generator {generator.Name} has invalid negative total heat input");
            }
            if (generator.ActualNetGeneration < 0)
            {
                throw new InvalidOperationException(
                    $"Coal generator {generator.Name} has invalid negative actual net generation");
            }
        }
    }
}
