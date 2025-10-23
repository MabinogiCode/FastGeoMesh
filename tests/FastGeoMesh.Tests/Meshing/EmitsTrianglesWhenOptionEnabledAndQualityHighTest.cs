using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using Xunit;

namespace FastGeoMesh.Tests.Meshing {
    public sealed class EmitsTrianglesWhenOptionEnabledAndQualityHighTest {
        [Fact]
        public void Test() {
            var outer = Polygon2D.FromPoints(new[] {
                new Vec2(0,0), new Vec2(6,0), new Vec2(6,2), new Vec2(4,2), new Vec2(4,4), new Vec2(6,4), new Vec2(6,6), new Vec2(0,6)
            });
            var structure = new PrismStructureDefinition(outer, 0, 1);
            var options = new MesherOptions {
                TargetEdgeLengthXY = EdgeLength.From(0.75),
                TargetEdgeLengthZ = EdgeLength.From(0.5),
                GenerateBottomCap = true,
                GenerateTopCap = true,
                MinCapQuadQuality = 0.95,
                OutputRejectedCapTriangles = true
            };
            var mesh = new PrismMesher().Mesh(structure, options).Value;
            Assert.True(mesh.Triangles.Count > 0, "Expected rejected cap triangles to be emitted");
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            Assert.True(im.Triangles.Count > 0, "Indexed mesh should retain triangle primitives");
            string tmp = Path.Combine(Path.GetTempPath(), $"fgm_tri_{Guid.NewGuid():N}.gltf");
            GltfExporter.Write(im, tmp);
            Assert.True(File.Exists(tmp));
            File.Delete(tmp);
        }
    }
}
