using FastGeoMesh.Application.Services;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Infrastructure.FileOperations;
using FastGeoMesh.Infrastructure.Services;

namespace FastGeoMesh.Sample
{
    /// <summary>
    /// Demonstrates the flexible TXT export system with custom formats.
    /// </summary>
    public static class FlexibleTxtExportExample
    {
        /// <summary>
        /// Shows various TXT export configurations with the builder pattern.
        /// </summary>
        public static void DemonstrateFlexibleTxtExport()
        {
            Console.WriteLine("🎯 Flexible TXT Export System Demo");

            // Create L-shaped structure for demonstration
            var lshape = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 3),
                new Vec2(3, 3), new Vec2(3, 6), new Vec2(0, 6)
            });
            var structure = new PrismStructureDefinition(lshape, 0, 4);

            var optionsResult = MesherOptions.CreateBuilder()
                .WithHighQualityPreset()
                .WithTargetEdgeLengthXY(0.8)
                .Build();

            if (optionsResult.IsSuccess)
            {
                var geometryService = new GeometryService();
                var zLevelBuilder = new ZLevelBuilder();
                var proximityChecker = new ProximityChecker();
                var mesher = new PrismMesher(geometryService, zLevelBuilder, proximityChecker);
                var meshResult = mesher.Mesh(structure, optionsResult.Value);
                if (meshResult.IsSuccess)
                {
                    var indexed = IndexedMesh.FromMesh(meshResult.Value);

                    // Example 1: Custom scientific format
                    Console.WriteLine("📄 Custom scientific format:");
                    indexed.ExportTxt()
                        .WithPoints("vertex", CountPlacement.Top, indexBased: true)
                        .WithEdges("edge", CountPlacement.None, indexBased: false)
                        .WithQuads("face", CountPlacement.Bottom, indexBased: true)
                        .ToFile("scientific_mesh.txt");
                    Console.WriteLine("   ✅ Created scientific_mesh.txt");

                    // Example 2: Minimal format (no indices, no counts)
                    Console.WriteLine("📄 Minimal format:");
                    indexed.ExportTxt()
                        .WithPoints("v", CountPlacement.None, indexBased: false)
                        .WithQuads("f", CountPlacement.None, indexBased: false)
                        .ToFile("minimal_mesh.txt");
                    Console.WriteLine("   ✅ Created minimal_mesh.txt");

                    // Example 3: Debug format with all counts at bottom
                    Console.WriteLine("📄 Debug format:");
                    indexed.ExportTxt()
                        .WithPoints("pt", CountPlacement.Bottom, indexBased: true)
                        .WithEdges("ln", CountPlacement.Bottom, indexBased: true)
                        .WithQuads("qd", CountPlacement.Bottom, indexBased: true)
                        .WithTriangles("tr", CountPlacement.Bottom, indexBased: true)
                        .ToFile("debug_mesh.txt");
                    Console.WriteLine("   ✅ Created debug_mesh.txt");

                    // Example 4: Use predefined formats
                    Console.WriteLine("📄 Predefined formats:");
                    TxtExporter.WriteObjLike(indexed, "objlike_mesh.txt");
                    Console.WriteLine("   ✅ Created objlike_mesh.txt");

                    Console.WriteLine($"📊 Mesh statistics: {indexed.VertexCount} vertices, {indexed.QuadCount} quads, {indexed.EdgeCount} edges");
                }
            }
        }

        /// <summary>
        /// Demonstrates reading and converting between formats.
        /// </summary>
        public static void DemonstrateFormatConversion()
        {
            Console.WriteLine("\n🔄 Format Conversion Demo");

            try
            {
                // Read Legacy format (the only text format that exists for reading)
                var mesh = IndexedMeshFileHelper.ReadCustomTxt("mesh.txt");
                Console.WriteLine($"📖 Read Legacy format: {mesh.VertexCount} vertices");

                // Convert to different TXT format
                mesh.ExportTxt()
                    .WithPoints("P", CountPlacement.Top, false)
                    .WithQuads("Q", CountPlacement.Top, false)
                    .ToFile("converted_mesh.txt");

                Console.WriteLine("✅ Converted to custom format: converted_mesh.txt");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("⚠️ Text mesh file not found. Create one first with the TXT exporter.");
            }
        }
    }
}
