using FastGeoMesh.Domain;
using System.Globalization;
using System.Text;

namespace FastGeoMesh.Meshing.Exporters
{
    /// <summary>Lightweight SVG top-view exporter (edges as lines).</summary>
    public static class SvgExporter
    {
        /// <summary>Write a top-view SVG of the mesh edges.</summary>
        public static void Write(IndexedMesh im, string path, double strokeWidth = 1.0, double? scale = null)
        {
            ArgumentNullException.ThrowIfNull(im);
            ArgumentNullException.ThrowIfNull(path);
            var verts = im.Vertices;
            if (verts.Count == 0)
            {
                File.WriteAllText(path, "<svg xmlns='http://www.w3.org/2000/svg' />", Encoding.UTF8);
                return;
            }

            var culture = CultureInfo.InvariantCulture;
            double minX = verts.Min(v => v.X);
            double maxX = verts.Max(v => v.X);
            double minY = verts.Min(v => v.Y);
            double maxY = verts.Max(v => v.Y);
            double spanX = Math.Max(1e-9, maxX - minX);
            double spanY = Math.Max(1e-9, maxY - minY);
            double spanMax = Math.Max(spanX, spanY);
            double sx = scale ?? (800.0 / spanMax);
            double sy = sx;

            string wStr = (spanX * sx).ToString(culture);
            string hStr = (spanY * sy).ToString(culture);
            string strokeStr = strokeWidth.ToString(culture);

            var sb = new StringBuilder();
            sb.Append("<svg xmlns='http://www.w3.org/2000/svg' width='").Append(wStr).Append("' height='").Append(hStr)
                  .Append("' viewBox='0 0 ").Append(wStr).Append(' ').Append(hStr).Append("' shape-rendering='crispEdges'>\n<g stroke='#222' stroke-width='")
                  .Append(strokeStr).Append("' fill='none'>\n");

            for (int ei = 0; ei < im.Edges.Count; ei++)
            {
                var e = im.Edges[ei];
                var va = verts[e.a];
                var vb = verts[e.b];
                string x1 = ((va.X - minX) * sx).ToString(culture);
                string y1 = ((maxY - va.Y) * sy).ToString(culture);
                string x2 = ((vb.X - minX) * sx).ToString(culture);
                string y2 = ((maxY - vb.Y) * sy).ToString(culture);
                sb.Append("<line x1='").Append(x1).Append("' y1='").Append(y1).Append("' x2='").Append(x2).Append("' y2='").Append(y2).Append("' />\n");
            }

            sb.Append("</g>\n</svg>\n");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }
    }
}
