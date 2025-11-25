using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Exporters
{
    /// <summary>
    /// Vérifie que l'export GLTF produit un fichier glTF valide avec buffer embarqué.
    /// </summary>
    public sealed class GltfExporterExportsGltfWithEmbeddedBuffer
    {
        [Fact]
        public void Test()
        {
            var poly = Polygon2D.FromPoints(new[] {
                new Vec2(0, 0),
                new Vec2(2, 0),
                new Vec2(2, 1),
                new Vec2(0, 1)
            });
            var st = new PrismStructureDefinition(poly, 0, 1);
            var opt = new MesherOptions
            {
                TargetEdgeLengthXY = TestMeshOptions.DefaultTargetEdgeLengthXY,
                TargetEdgeLengthZ = TestMeshOptions.DefaultTargetEdgeLengthZ
            };

            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var mesher = provider.GetRequiredService<IPrismMesher>();

            var mesh = mesher.Mesh(st, opt).UnwrapForTests();
            var im = IndexedMesh.FromMesh(mesh);

            string path = Path.Combine(Path.GetTempPath(), $"{TestFileConstants.TestFilePrefix}{Guid.NewGuid():N}.gltf");
            GltfExporter.Write(im, path);
            Assert.True(File.Exists(path));

            var json = File.ReadAllText(path);
            Assert.Contains("\"asset\"", json, StringComparison.Ordinal);
            Assert.Contains("\"buffers\"", json, StringComparison.Ordinal);
            Assert.Contains("data:application/octet-stream;base64,", json, StringComparison.Ordinal);
            File.Delete(path);
        }
    }
}
