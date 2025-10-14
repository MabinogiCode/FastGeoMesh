using FastGeoMesh.Domain;

namespace FastGeoMesh.Application.Helpers.Meshing
{
    /// <summary>Helper methods for generating prism side face quads.</summary>
    internal static class SideFaceMeshingHelper
    {
        /// <summary>Generates side face quads for a prism structure.</summary>
        internal static List<Quad> GenerateSideQuads(IReadOnlyList<Vec2> loop, IReadOnlyList<double> zLevels, MesherOptions options, bool outward)
        {
            ArgumentNullException.ThrowIfNull(loop);
            ArgumentNullException.ThrowIfNull(zLevels);
            ArgumentNullException.ThrowIfNull(options);

            var quads = new List<Quad>();
            int n = loop.Count;

            for (int i = 0; i < n; i++)
            {
                var a2 = loop[i];
                var b2 = loop[(i + 1) % n];
                double edgeLength = (b2 - a2).Length();
                int hDiv = Math.Max(1, (int)Math.Ceiling(edgeLength / options.TargetEdgeLengthXY.Value));
                double invHDiv = 1.0 / hDiv;

                for (int hi = 0; hi < hDiv; hi++)
                {
                    double t0 = hi * invHDiv;
                    double t1 = (hi + 1) * invHDiv;
                    var a0 = Helpers.GeometryCalculationHelper.Lerp(a2, b2, t0);
                    var a1 = Helpers.GeometryCalculationHelper.Lerp(a2, b2, t1);

                    for (int vi = 0; vi < zLevels.Count - 1; vi++)
                    {
                        double za0 = zLevels[vi];
                        double za1 = zLevels[vi + 1];

                        // FINAL SOLUTION: Fixed order guaranteeing positive cross product
                        // For a vertical quad, the 4 vertices in CCW order as seen from outside:
                        // bottom-left → bottom-right → top-right → top-left
                        var v0 = new Vec3(a0.X, a0.Y, za0);  // bottom-start
                        var v1 = new Vec3(a1.X, a1.Y, za0);  // bottom-end
                        var v2 = new Vec3(a1.X, a1.Y, za1);  // top-end
                        var v3 = new Vec3(a0.X, a0.Y, za1);  // top-start

                        Quad quad = outward
                            ? new Quad(v0, v1, v2, v3)  // No quality score for side quads
                            : new Quad(v0, v3, v2, v1); // No quality score for side quads

                        quads.Add(quad);
                    }
                }
            }
            return quads;
        }
    }
}
