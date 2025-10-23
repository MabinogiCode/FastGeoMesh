namespace FastGeoMesh.Domain {
    /// <summary>
    /// Represents the length of an edge, ensuring it is a positive, finite value within a valid range.
    /// </summary>
    public readonly record struct EdgeLength {
        /// <summary>
        /// The maximum allowed edge length value.
        /// </summary>
        public const double MaxValue = 1e6;

        /// <summary>
        /// The minimum allowed edge length value.
        /// </summary>
        public const double MinValue = 1e-6;

        /// <summary>
        /// Gets the value of the edge length.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeLength"/> struct.
        /// </summary>
        /// <param name="value">The edge length value.</param>
        /// <remarks>
        /// This constructor validates that <paramref name="value"/> is finite and within the allowed range.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if the value is not finite.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is outside the allowed range.</exception>
        private EdgeLength(double value) {
            if (double.IsNaN(value) || double.IsInfinity(value)) {
                throw new ArgumentException("Edge length must be a finite number.", nameof(value));
            }

            if (value < MinValue || value > MaxValue) {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"Edge length must be between {MinValue} and {MaxValue}.");
            }

            this.Value = value;
        }

        /// <summary>
        /// Creates an <see cref="EdgeLength"/> from a double value.
        /// </summary>
        /// <param name="value">The edge length value.</param>
        /// <returns>A new <see cref="EdgeLength"/> instance.</returns>
        public static EdgeLength From(double value) {
            return new EdgeLength(value);
        }

        /// <summary>
        /// Implicitly converts an <see cref="EdgeLength"/> to a <see cref="double"/>.
        /// </summary>
        /// <param name="length">The edge length to convert.</param>
        /// <returns>The underlying double value.</returns>
        public static implicit operator double(EdgeLength length) {
            return length.Value;
        }

        /// <summary>
        /// Returns a string representation of the edge length value.
        /// </summary>
        /// <returns>String representation of the numeric value.</returns>
        public override string ToString() {
            return this.Value.ToString();
        }
    }
}
