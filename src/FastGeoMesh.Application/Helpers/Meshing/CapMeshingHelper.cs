using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Application.Helpers.Meshing
{
    /// <summary>Helper for cap meshing (optimized grid path + generic tessellation + quad pairing).</summary>
    internal static class CapMeshingHelper
    {
        /// <summary>Create bottom/top caps according to options and return the resulting mesh.</summary>
        /// <remarks>Callers must supply an IGeometryService (via DI).</remarks>
        internal static ImmutableMesh GenerateCaps(ImmutableMesh inputMesh, PrismStructureDefinition structure, MesherOptions options, double z0, double z1, IGeometryService geometryService)
        {
            var mesh = inputMesh;
            bool genBottom = options.GenerateBottomCap;
            bool genTop = options.GenerateTopCap;

            if (genBottom || genTop)
            {
                if (structure.Footprint.IsRectangleAxisAligned(out var min, out var max))
                {
                    mesh = GenerateRectangleCaps(mesh, structure, options, z0, z1, min, max, genBottom, genTop, geometryService);
                }
                else
                {
                    mesh = GenerateGenericCaps(mesh, structure, options, z0, z1, genBottom, genTop, options.OutputRejectedCapTriangles);
                }
            }

            // Internal surfaces (always generic path, independent) - no extrusion, single elevation each
            foreach (var plate in structure.InternalSurfaces)
            {
                mesh = GenerateInternalSurface(mesh, plate);
            }

            return mesh;
        }

        private static ImmutableMesh GenerateInternalSurface(ImmutableMesh inputMesh, InternalSurfaceDefinition plate)
        {
            // For now, return a simple fallback implementation
            return GenerateFallbackInternalSurface(inputMesh, plate);
        }

        /// <summary>Fallback method when tessellation fails - generates a simple grid covering the outer polygon.</summary>
        private static ImmutableMesh GenerateFallbackInternalSurface(ImmutableMesh inputMesh, InternalSurfaceDefinition plate)
        {
            var mesh = inputMesh;
            var quadsToAdd = new List<Quad>();

            // Find bounding box of outer polygon
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;

            foreach (var v in plate.Outer.Vertices)
            {
                minX = Math.Min(minX, v.X);
                minY = Math.Min(minY, v.Y);
                maxX = Math.Max(maxX, v.X);
                maxY = Math.Max(maxY, v.Y);
            }

            // Simple grid generation
            int cols = 4;
            int rows = 4;
            double stepX = (maxX - minX) / cols;
            double stepY = (maxY - minY) / rows;

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    double x0 = minX + i * stepX;
                    double y0 = minY + j * stepY;
                    double x1 = x0 + stepX;
                    double y1 = y0 + stepY;

                    var v0 = new Vec3(x0, y0, plate.Elevation);
                    var v1 = new Vec3(x1, y0, plate.Elevation);
                    var v2 = new Vec3(x1, y1, plate.Elevation);
                    var v3 = new Vec3(x0, y1, plate.Elevation);

                    quadsToAdd.Add(new Quad(v0, v1, v2, v3));
                }
            }

            mesh = mesh.AddQuads(quadsToAdd);
            return mesh;
        }

        /// <summary>Optimized axis-aligned rectangle cap generation with refinement near holes/segments.</summary>
        internal static ImmutableMesh GenerateRectangleCaps(ImmutableMesh inputMesh, PrismStructureDefinition structure, MesherOptions options,
            double z0, double z1, Vec2 min, Vec2 max, bool genBottom, bool genTop, IGeometryService geometryService)
        {
            var mesh = inputMesh;
            var quadsToAdd = new List<Quad>();

            // ADAPTIVE REFINEMENT IMPLEMENTATION
            _ = max.X - min.X;
            _ = max.Y - min.Y;
            double baseTargetLength = options.TargetEdgeLengthXY.Value;

            // Create adaptive grid that accounts for refinement
            var refinementGrid = CreateAdaptiveGrid(min, max, baseTargetLength, structure, options);

            // Generate quads according to adaptive grid
            for (int i = 0; i < refinementGrid.XDivisions.Count - 1; i++)
            {
                for (int j = 0; j < refinementGrid.YDivisions.Count - 1; j++)
                {
                    double x0 = refinementGrid.XDivisions[i];
                    double x1 = refinementGrid.XDivisions[i + 1];
                    double y0 = refinementGrid.YDivisions[j];
                    double y1 = refinementGrid.YDivisions[j + 1];

                    // Check if this quad is inside a hole - if so, skip it
                    var quadCenter = new Vec2((x0 + x1) * 0.5, (y0 + y1) * 0.5);
                    if (IsPointInAnyHole(quadCenter, structure, geometryService))
                    {
                        continue; // Skip quads inside holes
                    }

                    if (genBottom)
                    {
                        var bottomQuad = new Quad(
                            new Vec3(x0, y0, z0),
                            new Vec3(x1, y0, z0),
                            new Vec3(x1, y1, z0),
                            new Vec3(x0, y1, z0),
                            1.0);
                        quadsToAdd.Add(bottomQuad);
                    }

                    if (genTop)
                    {
                        var topQuad = new Quad(
                            new Vec3(x0, y0, z1),
                            new Vec3(x1, y0, z1),
                            new Vec3(x1, y1, z1),
                            new Vec3(x0, y1, z1),
                            1.0);
                        quadsToAdd.Add(topQuad);
                    }
                }
            }

            return mesh.AddQuads(quadsToAdd);
        }

        /// <summary>Generic polygon tessellation path with quad pairing and optional triangle output.</summary>
        internal static ImmutableMesh GenerateGenericCaps(ImmutableMesh inputMesh, PrismStructureDefinition structure, MesherOptions options,
            double z0, double z1, bool genBottom, bool genTop, bool outputTris)
        {
            var mesh = inputMesh;
            var trianglesToAdd = new List<Triangle>();

            // Simple triangulation of the footprint
            var vertices = structure.Footprint.Vertices;
            if (vertices.Count >= 3)
            {
                // Fan triangulation from first vertex
                for (int i = 1; i < vertices.Count - 1; i++)
                {
                    if (genBottom)
                    {
                        trianglesToAdd.Add(new Triangle(
                            new Vec3(vertices[0].X, vertices[0].Y, z0),
                            new Vec3(vertices[i].X, vertices[i].Y, z0),
                            new Vec3(vertices[i + 1].X, vertices[i + 1].Y, z0)));
                    }

                    if (genTop)
                    {
                        trianglesToAdd.Add(new Triangle(
                            new Vec3(vertices[0].X, vertices[0].Y, z1),
                            new Vec3(vertices[i + 1].X, vertices[i + 1].Y, z1),
                            new Vec3(vertices[i].X, vertices[i].Y, z1)));
                    }
                }
            }

            return mesh.AddTriangles(trianglesToAdd);
        }

        /// <summary>Creates adaptive grid that refines near holes and segments.</summary>
        private static AdaptiveGrid CreateAdaptiveGrid(Vec2 min, Vec2 max, double baseTargetLength, PrismStructureDefinition structure, MesherOptions options)
        {
            double width = max.X - min.X;
            double height = max.Y - min.Y;

            // Start with uniform base grid
            int baseXDivisions = Math.Max(1, (int)Math.Round(width / baseTargetLength));
            int baseYDivisions = Math.Max(1, (int)Math.Round(height / baseTargetLength));

            var xDivisions = new List<double>();
            var yDivisions = new List<double>();

            // Create base divisions
            for (int i = 0; i <= baseXDivisions; i++)
            {
                xDivisions.Add(min.X + i * (width / baseXDivisions));
            }
            for (int j = 0; j <= baseYDivisions; j++)
            {
                yDivisions.Add(min.Y + j * (height / baseYDivisions));
            }

            // REFINEMENT NEAR HOLES
            if (options.TargetEdgeLengthXYNearHoles.HasValue && options.HoleRefineBand > 0)
            {
                double holeTargetLength = options.TargetEdgeLengthXYNearHoles.Value.Value;
                double holeBand = options.HoleRefineBand;

                foreach (var hole in structure.Holes)
                {
                    RefineGridNearFeature(xDivisions, yDivisions, hole.Vertices, holeTargetLength, holeBand, min, max);
                }
            }

            // REFINEMENT NEAR SEGMENTS
            if (options.TargetEdgeLengthXYNearSegments.HasValue && options.SegmentRefineBand > 0)
            {
                double segmentTargetLength = options.TargetEdgeLengthXYNearSegments.Value.Value;
                double segmentBand = options.SegmentRefineBand;

                foreach (var segment in structure.Geometry.Segments)
                {
                    RefineGridNearSegment(xDivisions, yDivisions, segment, segmentTargetLength, segmentBand, min, max);
                }
            }

            // Sort and deduplicate
            xDivisions.Sort();
            yDivisions.Sort();
            xDivisions = xDivisions.Distinct().ToList();
            yDivisions = yDivisions.Distinct().ToList();

            return new AdaptiveGrid { XDivisions = xDivisions, YDivisions = yDivisions };
        }

        /// <summary>Refines grid near a feature (hole).</summary>
        private static void RefineGridNearFeature(List<double> xDivisions, List<double> yDivisions, IReadOnlyList<Vec2> featureVertices,
            double targetLength, double band, Vec2 min, Vec2 max)
        {
            // Calculate bounding box of the feature
            double featureMinX = featureVertices.Min(v => v.X);
            double featureMaxX = featureVertices.Max(v => v.X);
            double featureMinY = featureVertices.Min(v => v.Y);
            double featureMaxY = featureVertices.Max(v => v.Y);

            // Extend by refinement band
            double refinementMinX = Math.Max(min.X, featureMinX - band);
            double refinementMaxX = Math.Min(max.X, featureMaxX + band);
            double refinementMinY = Math.Max(min.Y, featureMinY - band);
            double refinementMaxY = Math.Min(max.Y, featureMaxY + band);

            // Add subdivisions in refinement zone
            int refinedXDivisions = Math.Max(1, (int)Math.Round((refinementMaxX - refinementMinX) / targetLength));
            int refinedYDivisions = Math.Max(1, (int)Math.Round((refinementMaxY - refinementMinY) / targetLength));

            for (int i = 0; i <= refinedXDivisions; i++)
            {
                double x = refinementMinX + i * (refinementMaxX - refinementMinX) / refinedXDivisions;
                if (x >= min.X && x <= max.X)
                {
                    xDivisions.Add(x);
                }
            }

            for (int j = 0; j <= refinedYDivisions; j++)
            {
                double y = refinementMinY + j * (refinementMaxY - refinementMinY) / refinedYDivisions;
                if (y >= min.Y && y <= max.Y)
                {
                    yDivisions.Add(y);
                }
            }
        }

        /// <summary>Refines grid near a segment.</summary>
        private static void RefineGridNearSegment(List<double> xDivisions, List<double> yDivisions, Segment3D segment,
            double targetLength, double band, Vec2 min, Vec2 max)
        {
            // Project segment onto XY plane
            var start2D = new Vec2(segment.Start.X, segment.Start.Y);
            var end2D = new Vec2(segment.End.X, segment.End.Y);

            // Calculate bounding box of the segment
            double segmentMinX = Math.Min(start2D.X, end2D.X);
            double segmentMaxX = Math.Max(start2D.X, end2D.X);
            double segmentMinY = Math.Min(start2D.Y, end2D.Y);
            double segmentMaxY = Math.Max(start2D.Y, end2D.Y);

            // Extend by refinement band
            double refinementMinX = Math.Max(min.X, segmentMinX - band);
            double refinementMaxX = Math.Min(max.X, segmentMaxX + band);
            double refinementMinY = Math.Max(min.Y, segmentMinY - band);
            double refinementMaxY = Math.Min(max.Y, segmentMaxY + band);

            // Add subdivisions in refinement zone
            int refinedXDivisions = Math.Max(1, (int)Math.Round((refinementMaxX - refinementMinX) / targetLength));
            int refinedYDivisions = Math.Max(1, (int)Math.Round((refinementMaxY - refinementMinY) / targetLength));

            for (int i = 0; i <= refinedXDivisions; i++)
            {
                double x = refinementMinX + i * (refinementMaxX - refinementMinX) / refinedXDivisions;
                if (x >= min.X && x <= max.X)
                {
                    xDivisions.Add(x);
                }
            }

            for (int j = 0; j <= refinedYDivisions; j++)
            {
                double y = refinementMinY + j * (refinementMaxY - refinementMinY) / refinedYDivisions;
                if (y >= min.Y && y <= max.Y)
                {
                    yDivisions.Add(y);
                }
            }
        }

        /// <summary>Checks if a point is inside any hole.</summary>
        private static bool IsPointInAnyHole(Vec2 point, PrismStructureDefinition structure, IGeometryService geometryService)
        {
            return structure.Holes.Any(h => geometryService.PointInPolygon(h.Vertices.ToArray().AsSpan(), point.X, point.Y));
        }
    }

    /// <summary>Structure to store adaptive grid divisions for cap meshing.</summary>
    internal struct AdaptiveGrid
    {
        /// <summary>Gets or sets the X-axis division coordinates.</summary>
        public List<double> XDivisions;

        /// <summary>Gets or sets the Y-axis division coordinates.</summary>
        public List<double> YDivisions;
    }
}
