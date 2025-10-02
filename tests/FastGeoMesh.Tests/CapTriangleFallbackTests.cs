using FastGeoMesh.Geometry;
using FastGeoMesh.Meshing;
using FastGeoMesh.Meshing.Exporters;
using FastGeoMesh.Structures;
using Xunit;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// Tests ensuring cap triangle fallback emits triangles when quad quality filtering excludes them.
    /// </summary>
    public sealed class CapTriangleFallbackTests
    {
        /// <summary>
        /// Forces high minimum quad quality so triangles are emitted for rejected cap quads and validates glTF export.
        /// </summary>
        [Fact]
        public void EmitsTrianglesWhenOptionEnabledAndQualityHigh()
        {
            // Non-rectangle, with a re-entrant shape to force low quality pairs
            var outer = Polygon2D.FromPoints(new[] {
                new Vec2(0,0), new Vec2(6,0), new Vec2(6,2), new Vec2(4,2), new Vec2(4,4), new Vec2(6,4), new Vec2(6,6), new Vec2(0,6)
            });
            var structure = new PrismStructureDefinition(outer, 0, 1);
            var options = new MesherOptions
            {
                TargetEdgeLengthXY = 0.75,
                TargetEdgeLengthZ = 0.5,
                GenerateBottomCap = true,
                GenerateTopCap = true,
                MinCapQuadQuality = 0.95, // deliberately strict
                OutputRejectedCapTriangles = true
            };
            var mesh = new PrismMesher().Mesh(structure, options);
            // Expect some quads and some triangles
            Assert.True(mesh.Triangles.Count > 0, "Expected rejected cap triangles to be emitted");

            // Convert to indexed => triangles should be indexed too
            var im = IndexedMesh.FromMesh(mesh, options.Epsilon);
            Assert.True(im.Triangles.Count > 0, "Indexed mesh should retain triangle primitives");

            // glTF export should succeed
            string tmp = Path.Combine(Path.GetTempPath(), $"fgm_tri_{Guid.NewGuid():N}.gltf");
            GltfExporter.Write(im, tmp);
            Assert.True(File.Exists(tmp));
            File.Delete(tmp);
        }
    }
}
