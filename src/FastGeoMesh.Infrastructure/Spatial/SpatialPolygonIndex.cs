using FastGeoMesh.Domain;

namespace FastGeoMesh.Infrastructure
{
    /// <summary>Spatial acceleration structure for fast point-in-polygon queries.</summary>
    public sealed class SpatialPolygonIndex : ISpatialPolygonIndex
    {
        private readonly IReadOnlyList<Vec2> _vertices;
        private readonly double _minX, _maxX, _minY, _maxY;
        private readonly int _gridSize;
        private readonly double _cellSizeX, _cellSizeY;
        private readonly CellResult[][] _grid; // jagged array replacing 2D array
        private readonly Domain.IGeometryHelper _helper;

        /// <summary>Create spatial index for a polygon and inject a geometry helper for point-in-polygon checks.</summary>
        public SpatialPolygonIndex(IReadOnlyList<Vec2> vertices, Domain.IGeometryHelper helper, int gridResolution = 64)
        {
            ArgumentNullException.ThrowIfNull(vertices);
            ArgumentNullException.ThrowIfNull(helper);
            if (vertices.Count == 0)
            {
                throw new ArgumentException("Vertices collection must not be empty", nameof(vertices));
            }
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(gridResolution);

            _helper = helper;
            _vertices = vertices;
            _gridSize = gridResolution;

            // Compute bounding box
            _minX = _maxX = vertices[0].X;
            _minY = _maxY = vertices[0].Y;

            for (int i = 1; i < vertices.Count; i++)
            {
                var v = vertices[i];
                if (v.X < _minX) { _minX = v.X; }
                if (v.X > _maxX) { _maxX = v.X; }
                if (v.Y < _minY) { _minY = v.Y; }
                if (v.Y > _maxY) { _maxY = v.Y; }
            }

            // Add small margin to avoid edge cases
            double margin = Math.Max((_maxX - _minX), (_maxY - _minY)) * 0.01;
            _minX -= margin;
            _maxX += margin;
            _minY -= margin;
            _maxY += margin;

            _cellSizeX = (_maxX - _minX) / _gridSize;
            _cellSizeY = (_maxY - _minY) / _gridSize;
            _grid = new CellResult[_gridSize][];
            for (int i = 0; i < _gridSize; i++)
            {
                _grid[i] = new CellResult[_gridSize];
            }

            // Pre-compute grid cells
            BuildIndex();
        }

        /// <summary>Fast point-in-polygon test using spatial index.</summary>
        public bool IsInside(double x, double y)
        {
            // Check bounds
            if (x < _minX || x > _maxX || y < _minY || y > _maxY)
            {
                return false;
            }

            // If point lies exactly on any polygon edge treat as inside (robust edge handling)
            const double edgeTol = 1e-8;
            if (IsPointOnAnyEdge(x, y, edgeTol))
            {
                return true;
            }

            // Use injected helper for exact semantics
            return PointInPolygonRayCasting(x, y);
        }

        private bool IsPointOnAnyEdge(double x, double y, double tol)
        {
            int n = _vertices.Count;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var vi = _vertices[i];
                var vj = _vertices[j];
                if (IsPointOnSegment(x, y, vi.X, vi.Y, vj.X, vj.Y, tol))
                {
                    return true;
                }
            }
            return false;
        }

        private void BuildIndex()
        {
            // Sample each grid cell to determine if it's entirely inside, outside, or crosses boundary
            for (int i = 0; i < _gridSize; i++)
            {
                for (int j = 0; j < _gridSize; j++)
                {
                    double cellMinX = _minX + i * _cellSizeX;
                    double cellMaxX = _minX + (i + 1) * _cellSizeX;
                    double cellMinY = _minY + j * _cellSizeY;
                    double cellMaxY = _minY + (j + 1) * _cellSizeY;

                    // Sample corner points of cell
                    bool tl = PointInPolygonRayCasting(cellMinX, cellMinY);
                    bool tr = PointInPolygonRayCasting(cellMaxX, cellMinY);
                    bool bl = PointInPolygonRayCasting(cellMinX, cellMaxY);
                    bool br = PointInPolygonRayCasting(cellMaxX, cellMaxY);

                    if (tl && tr && bl && br)
                    {
                        _grid[i][j] = CellResult.Inside;
                    }
                    else if (!tl && !tr && !bl && !br)
                    {
                        // Check center point to be more conservative
                        double centerX = (cellMinX + cellMaxX) * 0.5;
                        double centerY = (cellMinY + cellMaxY) * 0.5;
                        bool centerInside = PointInPolygonRayCasting(centerX, centerY);

                        if (centerInside)
                        {
                            _grid[i][j] = CellResult.Boundary;
                        }
                        else
                        {
                            // Additionally verify no polygon edge intersects the cell and no vertex inside
                            if (DoesPolygonIntersectCell(cellMinX, cellMinY, cellMaxX, cellMaxY))
                            {
                                _grid[i][j] = CellResult.Boundary;
                            }
                            else
                            {
                                _grid[i][j] = CellResult.Outside;
                            }
                        }
                    }
                    else
                    {
                        _grid[i][j] = CellResult.Boundary;
                    }
                }
            }
        }

        private bool PointInPolygonRayCasting(double x, double y)
        {
            ReadOnlySpan<Vec2> span;
            if (_vertices is List<Vec2> list)
            {
                span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list);
            }
            else if (_vertices is Vec2[] array)
            {
                span = array.AsSpan();
            }
            else
            {
                span = _vertices.ToArray().AsSpan();
            }

            return _helper.PointInPolygon(span, x, y);
        }

        /// <summary>
        /// Determine if polygon has any vertex inside cell or any edge intersects cell rectangle.
        /// </summary>
        private bool DoesPolygonIntersectCell(double minX, double minY, double maxX, double maxY)
        {
            const double eps = 1e-9;
            // Check if any polygon vertex is inside the cell
            foreach (var v in _vertices)
            {
                if (v.X >= minX - eps && v.X <= maxX + eps && v.Y >= minY - eps && v.Y <= maxY + eps)
                {
                    return true;
                }
            }

            // Check each edge for intersection with cell rectangle's edges
            int n = _vertices.Count;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var a = _vertices[j];
                var b = _vertices[i];
                if (SegmentIntersectsRect(a.X, a.Y, b.X, b.Y, minX, minY, maxX, maxY))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SegmentIntersectsRect(double ax, double ay, double bx, double by, double minX, double minY, double maxX, double maxY)
        {
            const double eps = 1e-9;
            // If either endpoint inside rect -> intersects
            if ((ax >= minX - eps && ax <= maxX + eps && ay >= minY - eps && ay <= maxY + eps) ||
                (bx >= minX - eps && bx <= maxX + eps && by >= minY - eps && by <= maxY + eps))
            {
                return true;
            }

            // Check intersection with each rectangle edge
            if (SegmentsIntersect(ax, ay, bx, by, minX, minY, maxX, minY))
            {
                return true; // bottom
            }
            if (SegmentsIntersect(ax, ay, bx, by, maxX, minY, maxX, maxY))
            {
                return true; // right
            }
            if (SegmentsIntersect(ax, ay, bx, by, maxX, maxY, minX, maxY))
            {
                return true; // top
            }
            if (SegmentsIntersect(ax, ay, bx, by, minX, maxY, minX, minY))
            {
                return true; // left
            }

            return false;
        }

        private static bool SegmentsIntersect(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            const double eps = 1e-9;
            double o1 = Orientation(new Vec2(x1, y1), new Vec2(x2, y2), new Vec2(x3, y3));
            double o2 = Orientation(new Vec2(x1, y1), new Vec2(x2, y2), new Vec2(x4, y4));
            double o3 = Orientation(new Vec2(x3, y3), new Vec2(x4, y4), new Vec2(x1, y1));
            double o4 = Orientation(new Vec2(x3, y3), new Vec2(x4, y4), new Vec2(x2, y2));

            // General case
            if (o1 * o2 < -eps && o3 * o4 < -eps)
            {
                return true;
            }

            // Collinear or near-collinear cases
            if (Math.Abs(o1) <= eps && OnSegment(x1, y1, x3, y3, x2, y2, eps))
            {
                return true;
            }
            if (Math.Abs(o2) <= eps && OnSegment(x1, y1, x4, y4, x2, y2, eps))
            {
                return true;
            }
            if (Math.Abs(o3) <= eps && OnSegment(x3, y3, x1, y1, x4, y4, eps))
            {
                return true;
            }
            if (Math.Abs(o4) <= eps && OnSegment(x3, y3, x2, y2, x4, y4, eps))
            {
                return true;
            }

            return false;
        }

        private static double Orientation(in Vec2 a, in Vec2 b, in Vec2 c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        private static bool OnSegment(double x1, double y1, double x, double y, double x2, double y2, double eps)
        {
            return x >= Math.Min(x1, x2) - eps && x <= Math.Max(x1, x2) + eps && y >= Math.Min(y1, y2) - eps && y <= Math.Max(y1, y2) + eps;
        }

        /// <summary>Checks if a point lies on a line segment within tolerance.</summary>
        private static bool IsPointOnSegment(double px, double py, double ax, double ay, double bx, double by, double tolerance)
        {
            double apx = px - ax;
            double apy = py - ay;
            double abx = bx - ax;
            double aby = by - ay;

            double cross = Math.Abs(apx * aby - apy * abx);
            if (cross > tolerance)
            {
                return false;
            }

            double dot = apx * abx + apy * aby;
            double squaredLength = abx * abx + aby * aby;

            return dot >= -tolerance && dot <= squaredLength + tolerance;
        }
    }
}
