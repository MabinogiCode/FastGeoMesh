namespace FastGeoMesh.Domain {
    /// <summary>
    /// Represents a geometric tolerance, ensuring it is a positive and finite value within a valid range.
    /// </summary>
    public readonly record struct Tolerance {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tolerance"/> struct with validation.
        /// </summary>
        /// <param name="value">Tolerance value.</param>
        private Tolerance(double value) {
            if (double.IsNaN(value) || double.IsInfinity(value)) {
                throw new ArgumentException("Tolerance must be a finite number.", nameof(value));
            }

            if (value < MinValue || value > MaxValue) {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"Tolerance must be between {MinValue} and {MaxValue}.");
            }

            this.Value = value;
        }

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
        /// Creates a <see cref="Tolerance"/> from a double value.
        /// </summary>
        /// <param name="value">The tolerance value.</param>
        /// <returns>A new <see cref="Tolerance"/> instance.</returns>
        public static Tolerance From(double value) {
            return new Tolerance(value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="Tolerance"/> to a <see cref="double"/>.
        /// </summary>
        /// <param name="tolerance">The tolerance to convert.</param>
        /// <returns>The underlying <see cref="double"/> value.</returns>
        public static implicit operator double(Tolerance tolerance) {
            return tolerance.Value;
        }

        /// <summary>
        /// Returns a string representation of the tolerance value.
        /// </summary>
        /// <returns>String representation of the numeric tolerance.</returns>
        public override string ToString() {
            return this.Value.ToString();
        }
    }
}
