using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Performance
{
    /// <summary>
    /// Tests for class MesherOptionsBuilderCreatesValidOptionsTest.
    /// </summary>
    public sealed class MesherOptionsBuilderCreatesValidOptionsTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(1.0)
                .WithTargetEdgeLengthZ(0.5)
                .WithCaps(bottom: true, top: true)
                .WithHoleRefinement(0.5, 1.0)
                .WithMinCapQuadQuality(0.6)
                .WithRejectedCapTriangles(true)
                .Build().UnwrapForTests();

            options.TargetEdgeLengthXY.Value.Should().Be(1.0);
            options.TargetEdgeLengthZ.Value.Should().Be(0.5);
            options.GenerateBottomCap.Should().BeTrue();
            options.GenerateTopCap.Should().BeTrue();
            options.TargetEdgeLengthXYNearHoles?.Value.Should().Be(0.5);
            options.HoleRefineBand.Should().Be(1.0);
            options.MinCapQuadQuality.Should().Be(0.6);
            options.OutputRejectedCapTriangles.Should().BeTrue();
        }
    }
}
