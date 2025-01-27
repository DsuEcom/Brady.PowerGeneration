using Brady.PowerGeneration.Core.Enums;
using Brady.PowerGeneration.Core.Models;
using Brady.PowerGeneration.Core.Services.HelperServices;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brady.PowerGeneration.Tests.Calculators
{
    public class GenerationValueCalculatorTests
    {
        private readonly ILogger<GenerationValueCalculator> _logger;
        private readonly GenerationValueCalculator _calculator;
        private readonly ReferenceData _referenceData;

        public GenerationValueCalculatorTests()
        {
            _logger = Mock.Of<ILogger<GenerationValueCalculator>>();
            _calculator = new GenerationValueCalculator(_logger);
            _referenceData = new ReferenceData
            {
                Factors = new Factors
                {
                    ValueFactor = new ValueFactor
                    {
                        High = 0.946m,
                        Medium = 0.696m,
                        Low = 0.265m
                    }
                }
            };
        }

        [Theory]
        [InlineData(GeneratorType.OffshoreWind, 0.265)]   // Low factor
        [InlineData(GeneratorType.OnshoreWind, 0.946)]    // High factor
        [InlineData(GeneratorType.Gas, 0.696)]            // Medium factor
        [InlineData(GeneratorType.Coal, 0.696)]           // Medium factor
        public void GetValueFactor_ReturnsCorrectFactor(GeneratorType type, decimal expectedFactor)
        {
            // Act
            var result = _calculator.GetValueFactor(type, _referenceData);

            // Assert
            Assert.Equal(expectedFactor, result);
        }

        [Fact]
        public void CalculateGenerationValue_WithValidInputs_ReturnsCorrectValue()
        {
            // Arrange
            decimal energy = 100m;
            decimal price = 20m;
            var type = GeneratorType.OnshoreWind;
            decimal expectedValue = energy * price * 0.946m; // High factor for onshore wind

            // Act
            var result = _calculator.CalculateGenerationValue(energy, price, type, _referenceData);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineData(-1, 20)]  // Negative energy
        [InlineData(100, -1)] // Negative price
        public void CalculateGenerationValue_WithInvalidInputs_ThrowsArgumentException(
            decimal energy, decimal price)
        {
            // Arrange
            var type = GeneratorType.OnshoreWind;

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _calculator.CalculateGenerationValue(energy, price, type, _referenceData));
        }
    }
}
