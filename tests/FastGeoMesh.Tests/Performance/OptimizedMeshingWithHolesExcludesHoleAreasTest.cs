using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Interfaces;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Performance
{
    /// <summary>
    /// Tests for class OptimizedMeshingWithHolesExcludesHoleAreasTest.
    /// </summary>
    public sealed class OptimizedMeshingWithHolesExcludesHoleAreasTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(20, 0), new Vec2(20, 20), new Vec2(0, 20) });
            var hole = Polygon2D.FromPoints(new[] { new Vec2(5, 5), new Vec2(15, 5), new Vec2(15, 15), new Vec2(5, 15) });
            var structure = new PrismStructureDefinition(outer, 0, 2).AddHole(hole);
            var options = new MesherOptions { TargetEdgeLengthXY = EdgeLength.From(2.0), TargetEdgeLengthZ = EdgeLength.From(1.0), GenerateBottomCap = true, GenerateTopCap = true };
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var mesher = provider.GetRequiredService<IPrismMesher>();
            var mesh = mesher.Mesh(structure, options).UnwrapForTests();
            mesh.Quads.Should().NotBeEmpty();
            var capQuads = mesh.Quads.Where(q => System.Math.Abs(q.V0.Z - 0) < 0.1 || System.Math.Abs(q.V0.Z - 2) < 0.1).ToList();
            int quadsInHole = 0;
            int totalCapQuads = capQuads.Count;
            foreach (var quad in capQuads)
            {
                var centerX = (quad.V0.X + quad.V1.X + quad.V2.X + quad.V3.X) / 4.0;
                var centerY = (quad.V0.Y + quad.V1.Y + quad.V2.Y + quad.V3.Y) / 4.0;
                bool inHole = centerX > 6 && centerX < 14 && centerY > 6 && centerY < 14;
                if (inHole)
                {
                    quadsInHole++;
                }
            }
            if (totalCapQuads > 0)
            {
                double percentageInHole = (double)quadsInHole / totalCapQuads;
                percentageInHole.Should().BeLessThan(0.5);
            }
        }
    }
}
