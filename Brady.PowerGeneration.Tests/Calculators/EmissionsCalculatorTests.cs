using Brady.PowerGeneration.Core.Enums;
using Brady.PowerGeneration.Core.Models;
using Brady.PowerGeneration.Core.Services.HelperServices;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brady.PowerGeneration.Tests.Calculators
{
    public class EmissionsCalculatorTests
    {
        private readonly ILogger<EmissionsCalculator> _logger;
        private readonly EmissionsCalculator _calculator;
        private readonly ReferenceData _referenceData;

        public EmissionsCalculatorTests()
        {
            _logger = Mock.Of<ILogger<EmissionsCalculator>>();
            _calculator = new EmissionsCalculator(_logger);
            _referenceData = new ReferenceData
            {
                Factors = new Factors
                {
                    EmissionsFactor = new EmissionsFactor
                    {
                        High = 0.812m,
                        Medium = 0.562m,
                        Low = 0.312m
                    }
                }
            };
        }

        [Theory]
        [InlineData(GeneratorType.Gas, 0.562)]     // Medium factor
        [InlineData(GeneratorType.Coal, 0.812)]    // High factor
        [InlineData(GeneratorType.OnshoreWind, 0)] // No emissions
        public void GetEmissionFactor_ReturnsCorrectFactor(GeneratorType type, decimal expectedFactor)
        {
            // Act
            var result = _calculator.GetEmissionFactor(type, _referenceData);

            // Assert
            Assert.Equal(expectedFactor, result);
        }

        [Fact]
        public void CalculateEmissions_ForFossilFuelGenerator_ReturnsCorrectValue()
        {
            // Arrange
            decimal energy = 100m;
            decimal emissionsRating = 0.038m;
            var type = GeneratorType.Gas;
            decimal expectedEmissions = energy * emissionsRating * 0.562m; // Medium factor for gas

            // Act
            var result = _calculator.CalculateEmissions(energy, emissionsRating, type, _referenceData);

            // Assert
            Assert.Equal(expectedEmissions, result);
        }

        [Fact]
        public void CalculateEmissions_ForWindGenerator_ReturnsZero()
        {
            // Arrange
            decimal energy = 100m;
            decimal emissionsRating = 0m;
            var type = GeneratorType.OffshoreWind;

            // Act
            var result = _calculator.CalculateEmissions(energy, emissionsRating, type, _referenceData);

            // Assert
            Assert.Equal(0m, result);
        }
    }
}
