using FastGeoMesh.Domain.Validators;

namespace FastGeoMesh.Domain.Factories;
/// <summary>
/// Static factory class for creating Polygon2D instances with validation.
/// </summary>
public static class Polygon2DFactory
{
    /// <summary>
    /// Creates a polygon and validates the input; throws on invalid polygons.
    /// </summary>
    /// <param name="verts">The vertices of the polygon.</param>
    /// <returns>A validated Polygon2D instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when verts is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the polygon is invalid.</exception>
    public static Polygon2D CreateValidated(IEnumerable<Vec2> verts)
    {
        var list = (verts ?? throw new ArgumentNullException(nameof(verts))).ToList();
        if (list.Count < 3)
        {
            throw new ArgumentException("Polygon must have at least 3 vertices.", nameof(verts));
        }
        if (PolygonValidator.SignedArea(list) < 0)
        {
            list.Reverse();
        }
        if (!PolygonValidator.Validate(list, out var error))
        {
            throw new ArgumentException($"Invalid polygon: {error}", nameof(verts));
        }
        return new Polygon2D(list);
    }

    /// <summary>
    /// Unsafe factory that constructs a Polygon2D without performing validation.
    /// Use only when you know the input may be invalid (tests, diagnostics).
    /// </summary>
    /// <param name="verts">The vertices of the polygon.</param>
    /// <returns>A Polygon2D instance without validation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when verts is null.</exception>
    public static Polygon2D FromUnsafe(IEnumerable<Vec2> verts)
    {
        var list = (verts ?? throw new ArgumentNullException(nameof(verts))).ToList();
        if (list.Count >= 3 && PolygonValidator.SignedArea(list) < 0)
        {
            list.Reverse();
        }
        return new Polygon2D(list);
    }

    /// <summary>
    /// Tries to create a polygon without throwing; returns false and an error message on failure.
    /// </summary>
    /// <param name="verts">The vertices of the polygon.</param>
    /// <param name="polygon">The created polygon if successful; otherwise, null.</param>
    /// <param name="error">The error message if creation fails; otherwise, null.</param>
    /// <returns>True if the polygon was created successfully; otherwise, false.</returns>
    public static bool TryCreate(IEnumerable<Vec2> verts, out Polygon2D? polygon, out string? error)
    {
        polygon = null;
        error = null;
        if (verts == null)
        {
            error = "Vertices is null";
            return false;
        }
        var list = verts.ToList();
        if (list.Count < 3)
        {
            error = "Less than 3 vertices";
            return false;
        }
        if (PolygonValidator.SignedArea(list) < 0)
        {
            list.Reverse();
        }
        if (!PolygonValidator.Validate(list, out var verror))
        {
            error = verror;
            return false;
        }
        polygon = new Polygon2D(list);
        return true;
    }

    /// <summary>
    /// Helper construct from points. Behaves like the strict constructor (will validate).
    /// </summary>
    /// <param name="verts">The vertices of the polygon.</param>
    /// <returns>A validated Polygon2D instance.</returns>
    public static Polygon2D FromPoints(IEnumerable<Vec2> verts) => CreateValidated(verts);
}
