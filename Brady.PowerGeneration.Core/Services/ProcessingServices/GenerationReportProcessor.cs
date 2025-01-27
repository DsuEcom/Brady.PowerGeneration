using Brady.PowerGeneration.Core.Interfaces;
using Brady.PowerGeneration.Core.Models;
using Microsoft.Extensions.Logging;

namespace Brady.PowerGeneration.Core.Services.ProcessingServices
{
    public class GenerationReportProcessor : IGenerationReportProcessor
    {
        private readonly IValueFactorProvider _valueCalculator;
        private readonly IEmissionFactorProvider _emissionsCalculator;

        private readonly IXmlRepository<GenerationReport> _generationReportRepository;
        private readonly IXmlRepository<ReferenceData> _referenceRepository;
        private readonly IXmlRepository<GenerationReportDto> _outputRepository;

        private readonly ILogger<GenerationReportProcessor> _logger;

        public GenerationReportProcessor(
            IValueFactorProvider valueCalculator,
            IEmissionFactorProvider emissionsCalculator,
            IXmlRepository<GenerationReport> generationReportRepository,
            IXmlRepository<ReferenceData> referenceRepository,
            IXmlRepository<GenerationReportDto> outputRepository,
            ILogger<GenerationReportProcessor> logger)
        {
            _valueCalculator = valueCalculator;
            _emissionsCalculator = emissionsCalculator;
            _generationReportRepository = generationReportRepository;
            _outputRepository = outputRepository;
            _referenceRepository = referenceRepository;
            _logger = logger;
        }

        public async Task ProcessReportAsync(string inputFilePath, string outputFilePath, string referenceDataFilePath)
        {
            try
            {
                _logger.LogInformation("Starting report processing for file: {FilePath}", inputFilePath);

                var report = await _generationReportRepository.LoadAsync(inputFilePath);
                var referenceData = await _referenceRepository.LoadAsync(referenceDataFilePath);
                var output = CreateGenerationReportDto(report, referenceData);
                await _outputRepository.SaveAsync(output, outputFilePath);

                _logger.LogInformation("Successfully processed report and saved to: {FilePath}", outputFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing generation report");
                throw;
            }
        }

        private GenerationReportDto CreateGenerationReportDto(GenerationReport report, ReferenceData referenceData)
        {
            return new GenerationReportDto
            {
                Totals = new TotalsContainer
                {
                    Generator = CalculateGeneratorTotals(report, referenceData)
                },
                MaxEmissionGenerators = new MaxEmissionGeneratorsContainer
                {
                    Day = CalculateMaxEmissions(report, referenceData)
                },
                ActualHeatRates = new ActualHeatRatesContainer
                {
                    ActualHeatRate = CalculateHeatRates(report)
                }
            };
        }

        /// <summary>
        /// Calculates the total generation value for each generator in the report.
        /// The total is calculated by summing (Energy × Price × ValueFactor) for each day.
        /// </summary>
        private List<GeneratorTotal> CalculateGeneratorTotals(GenerationReport report, ReferenceData referenceData)
        {
            return report.GetAllGenerators()
                .Select(generator =>
                {
                    var total = generator.Generation?.Day.Sum(day =>
                        _valueCalculator.CalculateGenerationValue(
                            day.Energy,
                            day.Price,
                            generator.GetGeneratorType(),
                            referenceData));

                    return new GeneratorTotal
                    {
                        Name = generator.Name,
                        Total = total
                    };
                })
                .ToList();

        }

        /// <summary>
        /// Identifies the generator with the highest emissions for each day and calculates its emission value.
        /// Only considers fossil fuel generators (Gas and Coal) for emissions calculations.
        /// </summary>
        private List<MaxEmissionGenerator> CalculateMaxEmissions(GenerationReport report, ReferenceData referenceData)
        {
            return report.GetAllGenerators()
             .SelectMany(g => g.Generation!.Day.Select(d => new { Generator = g, Day = d }))
             .GroupBy(x => x.Day.Date.Date)
             .Select(group =>
             {
                 var maxEmission = group
                     .Where(x => x.Generator is GasGenerator or CoalGenerator)
                     .Select(x =>
                     {
                         var emission = _emissionsCalculator.CalculateEmissions(
                             x.Day.Energy,
                             ((dynamic)x.Generator).EmissionsRating,
                             x.Generator.GetGeneratorType(),
                             referenceData);

                         return new { x.Generator.Name, x.Day.Date, Emission = emission };
                     })
                     .MaxBy(x => x.Emission);

                 return new MaxEmissionGenerator
                 {
                     Name = maxEmission?.Name,
                     Date = maxEmission?.Date,
                     Emission = maxEmission?.Emission
                 };
             })
             .Where(x => x != null)
             .ToList();
        }  
       

        /// <summary>
        /// Calculates the actual heat rate for each coal generator.
        /// Heat Rate = TotalHeatInput / ActualNetGeneration
        /// Only applies to coal generators.
        /// </summary>
        private List<ActualHeatRate> CalculateHeatRates(GenerationReport report)
        {
            var heatRates = new List<ActualHeatRate>();

            if (report.Coal?.CoalGenerator != null)
            {
                foreach (var generator in report.Coal.CoalGenerator)
                {
                    var heatRate = generator.ActualNetGeneration != 0
                        ? generator.TotalHeatInput / generator.ActualNetGeneration
                        : 0;

                    heatRates.Add(new ActualHeatRate
                    {
                        Name = generator.Name,
                        HeatRate = heatRate
                    });
                }
            }

            return heatRates;
        }
        
    }
}
