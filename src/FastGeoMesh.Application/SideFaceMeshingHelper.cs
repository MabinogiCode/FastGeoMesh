using FastGeoMesh.Domain;
using FastGeoMesh.Meshing;
using System;
using System.Collections.Generic;

namespace FastGeoMesh.Meshing.Helpers
{
    /// <summary>Helper methods for generating prism side face quads.</summary>
    internal static class SideFaceMeshingHelper
    {
        internal static List<Quad> GenerateSideQuads(IReadOnlyList<Vec2> loop, IReadOnlyList<double> zLevels, MesherOptions options, bool outward)
        {
            ArgumentNullException.ThrowIfNull(loop);
            ArgumentNullException.ThrowIfNull(zLevels);
            ArgumentNullException.ThrowIfNull(options);
            int verticalSpans = Math.Max(1, zLevels.Count - 1);
            var quads = new List<Quad>(loop.Count * verticalSpans * 2);
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
                    var a0 = GeometryHelper.Lerp(a2, b2, t0);
                    var a1 = GeometryHelper.Lerp(a2, b2, t1);
                    for (int vi = 0; vi < zLevels.Count - 1; vi++)
                    {
                        double za0 = zLevels[vi];
                        double za1 = zLevels[vi + 1];
                        var v00 = new Vec3(a0.X, a0.Y, za0);
                        var v01 = new Vec3(a1.X, a1.Y, za0);
                        var v11 = new Vec3(a1.X, a1.Y, za1);
                        var v10 = new Vec3(a0.X, a0.Y, za1);
                        quads.Add(outward ? new Quad(v00, v01, v11, v10) : new Quad(v01, v00, v10, v11));
                    }
                }
            }
            return quads;
        }
    }
}
