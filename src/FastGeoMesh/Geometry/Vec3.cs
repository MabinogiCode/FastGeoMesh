namespace FastGeoMesh.Geometry;

/// <summary>3D vector.</summary>
public readonly record struct Vec3(double X, double Y, double Z)
{
    public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vec3 operator *(Vec3 a, double k) => new(a.X * k, a.Y * k, a.Z * k);

    // Friendly named alternatives for operators (CA2225)
    public static Vec3 Add(Vec3 a, Vec3 b) => a + b;
    public static Vec3 Subtract(Vec3 a, Vec3 b) => a - b;
    public static Vec3 Multiply(Vec3 a, double k) => a * k;
}
