namespace FastGeoMesh.Tests.Helpers
{
    /// <summary>
    /// Tests for class CIEnvironmentHelper.
    /// </summary>
    public static class CIEnvironmentHelper
    {
        /// <summary>
        /// Public API used by tests.
        /// </summary>
        public static bool IsCI =>
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_PIPELINES")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JENKINS_URL")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION"));
        /// <summary>
        /// Public API used by tests.
        /// </summary>
        public static double PerformanceMultiplier => IsCI ? 20.0 : 1.0;
        /// <summary>
        /// Runs test AdjustThreshold.
        /// </summary>
        public static double AdjustThreshold(double baseThreshold)
        {
            return baseThreshold * PerformanceMultiplier;
        }
        /// <summary>
        /// Runs test GetEnvironmentInfo.
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
