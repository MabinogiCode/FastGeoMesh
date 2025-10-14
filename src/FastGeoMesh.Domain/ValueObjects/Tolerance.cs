namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Represents a geometric tolerance, ensuring it is a positive and finite value within a valid range.
    /// </summary>
    public readonly record struct Tolerance
    {
        /// <summary>
        /// The maximum allowed tolerance value.
        /// </summary>
        public const double MaxValue = 1e-3;
        /// <summary>
        /// The minimum allowed tolerance value.
        /// </summary>
        public const double MinValue = 1e-12;

        /// <summary>
        /// Gets the value of the tolerance.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tolerance"/> struct with the specified value.
        /// </summary>
        /// <param name="value">The tolerance value.</param>
        /// <exception cref="ArgumentException">Thrown if the value is not finite.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is outside the allowed range.</exception>
        private Tolerance(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentException("Tolerance must be a finite number.", nameof(value));
            }
            if (value < MinValue || value > MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"Tolerance must be between {MinValue} and {MaxValue}.");
            }
            Value = value;
        }

        /// <summary>
        /// Creates a <see cref="Tolerance"/> from a double value.
        /// </summary>
        /// <param name="value">The tolerance value.</param>
        /// <returns>A new <see cref="Tolerance"/> instance.</returns>
        public static Tolerance From(double value)
        {
            return new Tolerance(value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="Tolerance"/> to a <see cref="double"/>.
        /// </summary>
        /// <param name="tolerance">The tolerance to convert.</param>
        public static implicit operator double(Tolerance tolerance) => tolerance.Value;

        /// <summary>
        /// Returns a string representation of the tolerance value.
        /// </summary>
        public override string ToString() => Value.ToString();
    }
}
