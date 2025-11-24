namespace FastGeoMesh.Domain
{
    /// <summary>Provides common geometric operations as an injectable service.</summary>
    public interface IGeometryHelper
    {
        /// <summary>Compute distance from a point to a line segment.</summary>
        double DistancePointToSegment(in Vec2 p, in Vec2 a, in Vec2 b, double tolerance = 0);
        /// <summary>Interpolate between two 2D points.</summary>
        Vec2 Lerp(in Vec2 a, in Vec2 b, double t);
        /// <summary>Scalar linear interpolation.</summary>
        double LerpScalar(double a, double b, double t);
        /// <summary>Check if a quad is convex.</summary>
        bool IsConvex((Vec2 a, Vec2 b, Vec2 c, Vec2 d) quad);
        /// <summary>Point-in-polygon test (point overload).</summary>
        bool PointInPolygon(ReadOnlySpan<Vec2> vertices, in Vec2 point, double tolerance = 0);
        /// <summary>Point-in-polygon test (xy overload).</summary>
        bool PointInPolygon(ReadOnlySpan<Vec2> vertices, double x, double y, double tolerance = 0);
        /// <summary>Batch point-in-polygon test.</summary>
        void BatchPointInPolygon(ReadOnlySpan<Vec2> vertices, ReadOnlySpan<Vec2> points, Span<bool> results, double tolerance = 0);
        /// <summary>Compute polygon area.</summary>
        double PolygonArea(ReadOnlySpan<Vec2> vertices);
    }
}
