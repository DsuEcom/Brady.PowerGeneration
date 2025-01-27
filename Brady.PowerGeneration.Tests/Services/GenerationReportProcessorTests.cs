using Brady.PowerGeneration.Core.Enums;
using Brady.PowerGeneration.Core.Interfaces;
using Brady.PowerGeneration.Core.Models;
using Brady.PowerGeneration.Core.Services.ProcessingServices;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brady.PowerGeneration.Tests.Services
{
    public class GenerationReportProcessorTests
    {
        private readonly Mock<IValueFactorProvider> _valueCalculatorMock;
        private readonly Mock<IEmissionFactorProvider> _emissionsCalculatorMock;
        private readonly Mock<IXmlRepository<GenerationReport>> _generationReportRepositoryMock;
        private readonly Mock<IXmlRepository<ReferenceData>> _referenceRepositoryMock;
        private readonly Mock<IXmlRepository<GenerationReportDto>> _outputRepositoryMock;
        private readonly Mock<ILogger<GenerationReportProcessor>> _loggerMock;
        private readonly GenerationReportProcessor _processor;

        public GenerationReportProcessorTests()
        {
            // Initialize all mocks
            _valueCalculatorMock = new Mock<IValueFactorProvider>();
            _emissionsCalculatorMock = new Mock<IEmissionFactorProvider>();
            _generationReportRepositoryMock = new Mock<IXmlRepository<GenerationReport>>();
            _referenceRepositoryMock = new Mock<IXmlRepository<ReferenceData>>();
            _outputRepositoryMock = new Mock<IXmlRepository<GenerationReportDto>>();
            _loggerMock = new Mock<ILogger<GenerationReportProcessor>>();

            // Create processor instance with mocked dependencies
            _processor = new GenerationReportProcessor(
                _valueCalculatorMock.Object,
                _emissionsCalculatorMock.Object,
                _generationReportRepositoryMock.Object,
                _referenceRepositoryMock.Object,
                _outputRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessReportAsync_WithValidInputs_GeneratesCorrectOutput()
        {
            // Arrange
            var inputFilePath = "input.xml";
            var outputFilePath = "output.xml";
            var referenceDataPath = "reference.xml";

            var generationReport = CreateSampleGenerationReport();
            var referenceData = CreateSampleReferenceData();

            // Setup repository mocks
            _generationReportRepositoryMock.Setup(x => x.LoadAsync(inputFilePath))
                .ReturnsAsync(generationReport);
            _referenceRepositoryMock.Setup(x => x.LoadAsync(referenceDataPath))
                .ReturnsAsync(referenceData);

            // Setup calculator mocks
            _valueCalculatorMock.Setup(x =>
                x.CalculateGenerationValue(
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<GeneratorType>(),
                    It.IsAny<ReferenceData>()))
                .Returns(100m); // Sample calculation result

            _emissionsCalculatorMock.Setup(x =>
                x.CalculateEmissions(
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<GeneratorType>(),
                    It.IsAny<ReferenceData>()))
                .Returns(50m); // Sample emission result

            // Act
            await _processor.ProcessReportAsync(inputFilePath, outputFilePath, referenceDataPath);

            // Assert
            _outputRepositoryMock.Verify(x =>
                x.SaveAsync(
                    It.Is<GenerationReportDto>(dto => ValidateGenerationReportDto(dto)),
                    outputFilePath),
                Times.Once);
        }

        [Fact]
        public async Task ProcessReportAsync_WhenGenerationReportLoadFails_ThrowsException()
        {
            // Arrange
            _generationReportRepositoryMock.Setup(x => x.LoadAsync(It.IsAny<string>()))
                .ThrowsAsync(new FileNotFoundException());

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                _processor.ProcessReportAsync("input.xml", "output.xml", "reference.xml"));
        }

        [Fact]
        public async Task ProcessReportAsync_WhenReferenceDataLoadFails_ThrowsException()
        {
            // Arrange
            _generationReportRepositoryMock.Setup(x => x.LoadAsync(It.IsAny<string>()))
                .ReturnsAsync(CreateSampleGenerationReport());
            _referenceRepositoryMock.Setup(x => x.LoadAsync(It.IsAny<string>()))
                .ThrowsAsync(new FileNotFoundException());

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                _processor.ProcessReportAsync("input.xml", "output.xml", "reference.xml"));
        }

        [Fact]
        public async Task ProcessReportAsync_CalculatesCorrectValues()
        {
            // Arrange
            var inputFilePath = "input.xml";
            var outputFilePath = "output.xml";
            var referenceDataPath = "reference.xml";

            var generationReport = CreateSampleGenerationReport();
            var referenceData = CreateSampleReferenceData();

            _generationReportRepositoryMock.Setup(x => x.LoadAsync(inputFilePath))
                .ReturnsAsync(generationReport);
            _referenceRepositoryMock.Setup(x => x.LoadAsync(referenceDataPath))
                .ReturnsAsync(referenceData);

            // Setup specific calculation results
            decimal expectedGenerationValue = 123.45m;
            _valueCalculatorMock.Setup(x =>
                x.CalculateGenerationValue(
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<GeneratorType>(),
                    It.IsAny<ReferenceData>()))
                .Returns(expectedGenerationValue);

            // Act
            await _processor.ProcessReportAsync(inputFilePath, outputFilePath, referenceDataPath);

            // Assert
            _outputRepositoryMock.Verify(x =>
                x.SaveAsync(
                    It.Is<GenerationReportDto>(dto =>
                        dto.Totals.Generator.Any(g => g.Total == expectedGenerationValue)),
                    outputFilePath),
                Times.Once);
        }

        private bool ValidateGenerationReportDto(GenerationReportDto dto)
        {
            return dto != null
                && dto.Totals?.Generator != null
                && dto.MaxEmissionGenerators?.Day != null
                && dto.ActualHeatRates?.ActualHeatRate != null;
        }

        private GenerationReport CreateSampleGenerationReport()
        {
            return new GenerationReport
            {
                Wind = new Wind
                {
                    WindGenerator = new List<WindGenerator>
                {
                    new WindGenerator
                    {
                        Name = "Wind[Offshore]",
                        Location = "Offshore",
                        Generation = new GenerationData
                        {
                            Day = new List<GenerationDay>
                            {
                                new GenerationDay
                                {
                                    Date = DateTime.Today,
                                    Energy = 100.368m,
                                    Price = 20.148m
                                }
                            }
                        }
                    }
                }
                },
                Gas = new Gas
                {
                    GasGenerator = new List<GasGenerator>
                {
                    new GasGenerator
                    {
                        Name = "Gas[1]",
                        EmissionsRating = 0.038m,
                        Generation = new GenerationData
                        {
                            Day = new List<GenerationDay>
                            {
                                new GenerationDay
                                {
                                    Date = DateTime.Today,
                                    Energy = 200m,
                                    Price = 15m
                                }
                            }
                        }
                    }
                }
                }
            };
        }

        private ReferenceData CreateSampleReferenceData()
        {
            return new ReferenceData
            {
                Factors = new Factors
                {
                    ValueFactor = new ValueFactor
                    {
                        High = 0.946m,
                        Medium = 0.696m,
                        Low = 0.265m
                    },
                    EmissionsFactor = new EmissionsFactor
                    {
                        High = 0.812m,
                        Medium = 0.562m,
                        Low = 0.312m
                    }
                }
            };
        }
    }
}