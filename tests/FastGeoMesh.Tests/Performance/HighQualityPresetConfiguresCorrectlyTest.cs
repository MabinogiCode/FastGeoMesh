using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Performance
{
    /// <summary>
    /// Tests for class HighQualityPresetConfiguresCorrectlyTest.
    /// </summary>
    public sealed class HighQualityPresetConfiguresCorrectlyTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var options = MesherOptions.CreateBuilder()
                .WithHighQualityPreset()
                .Build().UnwrapForTests();

            options.TargetEdgeLengthXY.Value.Should().Be(0.5);
            options.TargetEdgeLengthZ.Value.Should().Be(0.5);
            options.MinCapQuadQuality.Should().Be(0.7);
            options.OutputRejectedCapTriangles.Should().BeTrue();
        }
    }
}
