using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Performance
{
    public sealed class FastPresetConfiguresCorrectlyTest
    {
        [Fact]
        public void Test()
        {
            var options = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .Build().UnwrapForTests();

            options.TargetEdgeLengthXY.Value.Should().Be(2.0);
            options.TargetEdgeLengthZ.Value.Should().Be(2.0);
            options.MinCapQuadQuality.Should().Be(0.3);
            options.OutputRejectedCapTriangles.Should().BeFalse();
        }
    }
}
