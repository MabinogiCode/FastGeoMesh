using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Validation
{
    public sealed class V2ResultPatternOptionsValidationWorks
    {
        [Fact]
        public void Test()
        {
            // Arrange & Act - Valid options
            var validResult = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(1.0)
                .WithTargetEdgeLengthZ(1.0)
                .Build();

            // Assert
            validResult.IsSuccess.Should().BeTrue();
            validResult.Value.Should().NotBeNull();

            // Arrange & Act - Invalid options
            var invalidResult = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(-1.0) // Invalid negative value
                .Build();

            // Assert
            invalidResult.IsFailure.Should().BeTrue();
            invalidResult.Error.Description.Should().NotBeEmpty();
        }
    }
}
