namespace FastGeoMesh.Geometry
{
    /// <summary>3D vector.</summary>
    public readonly record struct Vec3(double X, double Y, double Z)
    {
        /// <summary>Add two vectors.</summary>
        public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        /// <summary>Subtract b from a.</summary>
        public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        /// <summary>Scale vector by k.</summary>
        public static Vec3 operator *(Vec3 a, double k) => new(a.X * k, a.Y * k, a.Z * k);

        /// <inheritdoc cref="op_Addition" />
        public static Vec3 Add(Vec3 a, Vec3 b) => a + b;
        /// <inheritdoc cref="op_Subtraction" />
        public static Vec3 Subtract(Vec3 a, Vec3 b) => a - b;
        /// <inheritdoc cref="op_Multiply" />
        public static Vec3 Multiply(Vec3 a, double k) => a * k;
    }
}
