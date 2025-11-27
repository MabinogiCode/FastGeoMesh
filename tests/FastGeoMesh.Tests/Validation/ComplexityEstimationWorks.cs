using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Interfaces;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Validation
{
    /// <summary>
    /// Tests for class ComplexityEstimationWorks.
    /// </summary>
    public sealed class ComplexityEstimationWorks
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public async Task Test()
        {
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build().UnwrapForTests();
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var mesher = provider.GetRequiredService<IPrismMesher>();
            var asyncMesher = (IAsyncMesher)mesher;
            var estimate = await asyncMesher.EstimateComplexityAsync(structure, options);
            estimate.Should().NotBeNull();
            estimate.EstimatedQuadCount.Should().BeGreaterThan(0);
            estimate.EstimatedTriangleCount.Should().BeGreaterThan(0);
            estimate.EstimatedComputationTime.Should().BeGreaterThan(TimeSpan.Zero);
            estimate.Complexity.Should().BeOneOf(MeshingComplexity.Trivial, MeshingComplexity.Simple);
        }
    }
}
