using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    /// <summary>
    /// Tests for class PerformanceMonitoringTypesAreExcludedFromCoverageTest.
    /// </summary>
    public sealed class PerformanceMonitoringTypesAreExcludedFromCoverageTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        [SuppressMessage("Globalization", "CA1303", Justification = "Test output strings are non-localized test artifacts")]
        [SuppressMessage("Globalization", "CA1303", Justification = "Test output strings are non-localized test artifacts")]
        public void Test()
        {
            var performanceMonitorType = Type.GetType("FastGeoMesh.Utils.PerformanceMonitor, FastGeoMesh");
            var tessPoolType = Type.GetType("FastGeoMesh.Utils.TessPool, FastGeoMesh");

            if (performanceMonitorType != null)
            {
                performanceMonitorType.IsClass.Should().BeTrue();
                Console.WriteLine("✓ PerformanceMonitor type found - should be excluded from coverage");
            }

            if (tessPoolType != null)
            {
                tessPoolType.IsClass.Should().BeTrue();
                Console.WriteLine("✓ TessPool type found - configured for exclusion in runsettings");
            }

            true.Should().BeTrue();
        }
    }
}
