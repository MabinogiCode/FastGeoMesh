namespace FastGeoMesh.Tests.Helpers
{
    /// <summary>
    /// Helper to detect CI environments and adjust performance thresholds accordingly.
    /// </summary>
    public static class CIEnvironmentHelper
    {
        /// <summary>
        /// Detects if we're running in a CI environment.
        /// </summary>
        public static bool IsCI =>
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_PIPELINES")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JENKINS_URL")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION"));

        /// <summary>
        /// Gets a multiplier for performance thresholds based on environment.
        /// CI environments get much more lenient thresholds due to variable performance.
        /// </summary>
        public static double PerformanceMultiplier => IsCI ? 20.0 : 1.0;

        /// <summary>
        /// Adjusts a base threshold for the current environment.
        /// </summary>
        /// <param name="baseThreshold">Base threshold for development environment</param>
        /// <returns>Adjusted threshold for current environment</returns>
        public static double AdjustThreshold(double baseThreshold)
        {
            return baseThreshold * PerformanceMultiplier;
        }

        /// <summary>
        /// Gets environment info for logging.
        /// </summary>
        public static string GetEnvironmentInfo()
        {
            if (IsCI)
            {
                var ciProvider = GetCIProvider();
                return $"CI Environment: {ciProvider} (Performance Multiplier: {PerformanceMultiplier}x)";
            }
            return "Development Environment (Performance Multiplier: 1x)";
        }

        private static string GetCIProvider()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")))
            {
                return "GitHub Actions";
            }
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_PIPELINES")))
            {
                return "Azure Pipelines";
            }
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JENKINS_URL")))
            {
                return "Jenkins";
            }
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
            {
                return "TeamCity";
            }
            return "Unknown CI";
        }
    }
}
