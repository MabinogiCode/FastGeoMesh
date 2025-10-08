using FastGeoMesh.Application;
using FastGeoMesh.Infrastructure;

namespace FastGeoMesh.Sample
{
    /// <summary>
    /// Demonstrates Legacy format export capabilities in FastGeoMesh v2.0.
    /// The Legacy format is the primary format for FastGeoMesh library.
    /// </summary>
    public static class LegacyFormatExample
    {
        /// <summary>
        /// Demonstrates basic Legacy format export.
        /// </summary>
        public static void DemonstrateBasicLegacyExport()
        {
            Console.WriteLine("📄 Legacy Format Export (Primary FastGeoMesh Format)");

            // Domain: Create a simple rectangular structure
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 3);

            // Application: Configure meshing with fast preset
            var optionsResult = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .WithTargetEdgeLengthXY(1.0)
                .Build();

            if (optionsResult.IsSuccess)
            {
                var mesher = new PrismMesher();
                var meshResult = mesher.Mesh(structure, optionsResult.Value);

                if (meshResult.IsSuccess)
                {
                    var mesh = meshResult.Value;
                    var indexed = IndexedMesh.FromMesh(mesh);

                    // Infrastructure: Export to Legacy format (primary format)
                    LegacyExporter.Write(indexed, "example_mesh.txt");

                    // Or use legacy naming convention
                    LegacyExporter.WriteWithLegacyName(indexed, "./output/");

                    Console.WriteLine($"✅ Legacy format exported: {indexed.VertexCount} vertices, {indexed.QuadCount} quads");
                    Console.WriteLine($"📁 Files created: example_mesh.txt and ./output/0_maill.txt");
                }
            }
        }

        /// <summary>
        /// Demonstrates reading Legacy format files.
        /// </summary>
        public static void DemonstrateLegacyImport()
        {
            Console.WriteLine("📖 Legacy Format Import");

            try
            {
                // Domain: Read Legacy format file
                var mesh = IndexedMeshFileHelper.ReadCustomTxt("example_mesh.txt");

                Console.WriteLine($"✅ Legacy format imported: {mesh.VertexCount} vertices, {mesh.QuadCount} quads, {mesh.EdgeCount} edges");

                // The imported mesh can be used for further processing
                Console.WriteLine("📊 Mesh statistics:");
                Console.WriteLine($"   Vertices: {mesh.VertexCount}");
                Console.WriteLine($"   Edges: {mesh.EdgeCount}");
                Console.WriteLine($"   Quads: {mesh.QuadCount}");
                Console.WriteLine($"   Triangles: {mesh.TriangleCount}");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("⚠️ Legacy file not found. Run DemonstrateBasicLegacyExport() first.");
            }
        }

        /// <summary>
        /// Demonstrates round-trip Legacy format processing.
        /// </summary>
        public static void DemonstrateLegacyRoundTrip()
        {
            Console.WriteLine("🔄 Legacy Format Round-Trip Processing");

            // 1. Create original mesh
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 3), new Vec2(0, 3)
            });
            var structure = new PrismStructureDefinition(polygon, -1, 2);

            var optionsResult = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(1.0)
                .WithTargetEdgeLengthZ(1.0)
                .Build();

            if (optionsResult.IsSuccess)
            {
                var mesher = new PrismMesher();
                var meshResult = mesher.Mesh(structure, optionsResult.Value);

                if (meshResult.IsSuccess)
                {
                    var originalMesh = meshResult.Value;
                    var originalIndexed = IndexedMesh.FromMesh(originalMesh);

                    Console.WriteLine($"📊 Original mesh: {originalIndexed.VertexCount} vertices, {originalIndexed.QuadCount} quads");

                    // 2. Export to Legacy format
                    LegacyExporter.Write(originalIndexed, "roundtrip_test.txt");
                    Console.WriteLine("💾 Exported to Legacy format");

                    // 3. Import from Legacy format
                    var importedIndexed = IndexedMeshFileHelper.ReadCustomTxt("roundtrip_test.txt");
                    Console.WriteLine($"📊 Imported mesh: {importedIndexed.VertexCount} vertices, {importedIndexed.QuadCount} quads");

                    // 4. Verify data integrity
                    bool dataPreserved = originalIndexed.VertexCount == importedIndexed.VertexCount &&
                                       originalIndexed.QuadCount == importedIndexed.QuadCount &&
                                       originalIndexed.EdgeCount == importedIndexed.EdgeCount;

                    Console.WriteLine($"✅ Data integrity: {(dataPreserved ? "PRESERVED" : "COMPROMISED")}");

                    // 5. Export to other formats for comparison
                    ObjExporter.Write(importedIndexed, "roundtrip_result.obj");
                    Console.WriteLine("📄 Also exported to OBJ format for comparison");
                }
            }
        }

        /// <summary>
        /// Demonstrates Legacy format with complex geometry.
        /// </summary>
        public static void DemonstrateLegacyWithComplexGeometry()
        {
            Console.WriteLine("🏗️ Legacy Format with Complex Geometry");

            // Create L-shaped structure
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
                var mesher = new PrismMesher();
                var meshResult = mesher.Mesh(structure, optionsResult.Value);

                if (meshResult.IsSuccess)
                {
                    var mesh = meshResult.Value;
                    var indexed = IndexedMesh.FromMesh(mesh);

                    // Export complex geometry to Legacy format
                    LegacyExporter.WriteWithLegacyName(indexed, "./complex_output/");

                    Console.WriteLine($"✅ Complex geometry exported:");
                    Console.WriteLine($"   📊 {indexed.VertexCount} vertices");
                    Console.WriteLine($"   📊 {indexed.QuadCount} quads");
                    Console.WriteLine($"   📊 {indexed.EdgeCount} edges");
                    Console.WriteLine($"   📁 File: ./complex_output/0_maill.txt");
                }
            }
        }
    }
}
