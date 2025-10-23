namespace FastGeoMesh.Domain.Services {
    /// <summary>
    /// Abstraction for obtaining current time in a testable way.
    /// Use this to avoid direct DateTime.Now/UtcNow calls in production code and enable deterministic tests.
    /// </summary>
    public interface IClock {
        /// <summary>
        /// Gets the current moment in Coordinated Universal Time (UTC).
        /// </summary>
        DateTime UtcNow { get; }

        /// <summary>
        /// Gets the current local time for the system.
        /// </summary>
        DateTime Now { get; }
    }
}
