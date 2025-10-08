using FastGeoMesh.Domain;
using FastGeoMesh.Utils;

namespace FastGeoMesh.Infrastructure
{
    /// <summary>Spatial acceleration structure for fast point-in-polygon queries.</summary>
    public sealed class SpatialPolygonIndex
    {
        private readonly IReadOnlyList<Vec2> _vertices;
        private readonly double _minX, _maxX, _minY, _maxY;
        private readonly int _gridSize;
        private readonly double _cellSizeX, _cellSizeY;
        private readonly CellResult[][] _grid; // jagged array replacing 2D array

        /// <summary>Create spatial index for a polygon.</summary>
        public SpatialPolygonIndex(IReadOnlyList<Vec2> vertices, int gridResolution = 64)
        {
            ArgumentNullException.ThrowIfNull(vertices);
            if (vertices.Count == 0)
            {
                throw new ArgumentException("Vertices collection must not be empty", nameof(vertices));
            }
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(gridResolution);
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

            // Get grid cell
            int cellX = Math.Min(_gridSize - 1, (int)((x - _minX) / _cellSizeX));
            int cellY = Math.Min(_gridSize - 1, (int)((y - _minY) / _cellSizeY));

            var cellResult = _grid[cellX][cellY];

            return cellResult switch
            {
                CellResult.Inside => true,
                CellResult.Outside => false,
                _ => PointInPolygonRayCasting(x, y)
            };
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
                        _grid[i][j] = PointInPolygonRayCasting(centerX, centerY) ? CellResult.Boundary : CellResult.Outside;
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
            // Convert IReadOnlyList to ReadOnlySpan for optimal performance
            ReadOnlySpan<Vec2> span = _vertices is List<Vec2> list
                ? System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list)
                : _vertices is Vec2[] array
                    ? array.AsSpan()
                    : _vertices.ToArray().AsSpan();

            return GeometryHelper.PointInPolygon(span, x, y);
        }
    }
}
