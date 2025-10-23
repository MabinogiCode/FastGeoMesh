using FastGeoMesh.Domain.Services;

namespace FastGeoMesh.Infrastructure.Services {
    /// <summary>
    /// Production implementation of <see cref="IClock"/> using system time.
    /// </summary>
    public sealed class SystemClock : IClock {
        /// <summary>Current UTC time.</summary>
        public DateTime UtcNow => DateTime.UtcNow;

        /// <summary>Current local time.</summary>
        public DateTime Now => DateTime.Now;
    }
}
