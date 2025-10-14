using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    /// <summary>
    /// Tests edge cases, error conditions, and boundary scenarios to improve code coverage.
    /// Focuses on paths that are typically under-tested in normal scenarios.
    /// </summary>
    public sealed class EdgeCaseCoverageTests
    {
        private readonly MesherOptions _options;

        /// <summary>Initializes the test class with options.</summary>
        public EdgeCaseCoverageTests()
        {
            _options = MesherOptions.CreateBuilder().WithFastPreset().Build().UnwrapForTests();
        }

        /// <summary>Tests PrismMesher with custom cap strategy.</summary>
        [Fact]
        public void PrismMesherWithCustomCapStrategyUsesCustomStrategy()
        {
            // Arrange
            var customStrategy = new CustomTestCapStrategy();
            var mesher = new PrismMesher(customStrategy);

            var structure = new PrismStructureDefinition(
                Polygon2D.FromPoints(new[]
                {
                    new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
                }), 0, 2);

            // Act
            var mesh = mesher.Mesh(structure, _options).UnwrapForTests();

            // Assert
            mesh.Should().NotBeNull();
            customStrategy.WasCalled.Should().BeTrue("Custom strategy should have been used");
        }

        /// <summary>Tests null cap strategy validation.</summary>
        [Fact]
        public void PrismMesherWithNullCapStrategyThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PrismMesher(null!));
        }

        /// <summary>Tests MesherOptions validation with invalid values.</summary>
        [Fact]
        public void MesherOptionsWithInvalidValuesFailsValidation()
        {
            // Act
            var result = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(-1.0) // Invalid negative value
                .Build();

            // Assert
            result.IsFailure.Should().BeTrue("Should fail validation with invalid values");
            result.Error.Description.Should().NotBeEmpty("Should provide error description");
        }
    }
}

