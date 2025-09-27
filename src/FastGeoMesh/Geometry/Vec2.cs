using System;

namespace FastGeoMesh.Geometry
{
    /// <summary>2D vector in XY plane.</summary>
    public readonly record struct Vec2(double X, double Y)
    {
        /// <summary>Add two vectors.</summary>
        public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
        /// <summary>Subtract b from a.</summary>
        public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
        /// <summary>Scale vector by k.</summary>
        public static Vec2 operator *(Vec2 a, double k) => new(a.X * k, a.Y * k);

        /// <inheritdoc cref="op_Addition" />
        public static Vec2 Add(Vec2 a, Vec2 b) => a + b;
        /// <inheritdoc cref="op_Subtraction" />
        public static Vec2 Subtract(Vec2 a, Vec2 b) => a - b;
        /// <inheritdoc cref="op_Multiply" />
        public static Vec2 Multiply(Vec2 a, double k) => a * k;

        /// <summary>Dot product with b.</summary>
        public double Dot(in Vec2 b) => X * b.X + Y * b.Y;
        /// <summary>Z component of 2D cross (a x b).</summary>
        public double Cross(in Vec2 b) => X * b.Y - Y * b.X;
        /// <summary>Euclidean length.</summary>
        public double Length() => Math.Sqrt(X * X + Y * Y);
    }
}
