namespace FastGeoMesh.Infrastructure.Services
{
    /// <summary>
    /// Wrapper to make Activity disposable for proper resource management.
    /// </summary>
    internal sealed class ActivityDisposable : IDisposable
    {
        private readonly System.Diagnostics.Activity? _activity;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityDisposable"/> class.
        /// </summary>
        /// <param name="activity">The activity to wrap.</param>
        public ActivityDisposable(System.Diagnostics.Activity? activity)
        {
            _activity = activity;
        }

        /// <summary>
        /// Disposes the wrapped activity.
        /// </summary>
        public void Dispose()
        {
            _activity?.Dispose();
        }
    }
}
