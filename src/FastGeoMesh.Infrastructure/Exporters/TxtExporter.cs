using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure.Exporters;

namespace FastGeoMesh.Infrastructure
{
    /// <summary>Flexible TXT exporter with configurable format.</summary>
    public static class TxtExporter
    {
        /// <summary>Start building a TXT export configuration.</summary>
        public static TxtExportBuilder ExportTxt(this IndexedMesh mesh)
        {
            return new TxtExportBuilder(mesh);
        }

        /// <summary>Export with OBJ-like format (no counts, prefixed elements).</summary>
        public static void WriteObjLike(IndexedMesh mesh, string filePath)
        {
            mesh.ExportTxt()
                .WithPoints("v", CountPlacement.None, false)
                .WithEdges("l", CountPlacement.None, false)
                .WithQuads("f", CountPlacement.None, false)
                .WithTriangles("f", CountPlacement.None, false)
                .ToFile(filePath);
        }
    }
}
