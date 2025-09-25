namespace FastGeoMesh.Geometry;

/// <summary>2D vector in XY plane.</summary>
public readonly record struct Vec2(double X, double Y)
{
    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vec2 operator *(Vec2 a, double k) => new(a.X * k, a.Y * k);

    // Friendly named alternatives for operators (CA2225)
    public static Vec2 Add(Vec2 a, Vec2 b) => a + b;
    public static Vec2 Subtract(Vec2 a, Vec2 b) => a - b;
    public static Vec2 Multiply(Vec2 a, double k) => a * k;

    public double Dot(in Vec2 b) => X * b.X + Y * b.Y;
    public double Cross(in Vec2 b) => X * b.Y - Y * b.X; // z-component
    public double Length() => Math.Sqrt(X*X + Y*Y);
}
