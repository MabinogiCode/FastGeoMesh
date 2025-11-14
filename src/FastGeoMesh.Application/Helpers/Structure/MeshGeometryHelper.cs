using FastGeoMesh.Domain;

namespace FastGeoMesh.Application.Helpers.Structure
{
    /// <summary>Helper class for mesh geometry operations.</summary>
    internal static class MeshGeometryHelper
    {
        /// <summary>Computes the area of a polygon using the Shoelace formula.</summary>
        internal static double ComputeArea(Polygon2D polygon)
        {
            if (polygon == null || polygon.Vertices == null || polygon.Vertices.Count < 3)
            {
                return 0.0;
            }

            var vertices = polygon.Vertices;
            double area = 0.0;

            // Shoelace formula (also known as the surveyor's formula)
            for (int i = 0; i < vertices.Count; i++)
            {
                int j = (i + 1) % vertices.Count;
                area += vertices[i].X * vertices[j].Y;
                area -= vertices[j].X * vertices[i].Y;
            }

            return Math.Abs(area) / 2.0;
        }
    }
}
