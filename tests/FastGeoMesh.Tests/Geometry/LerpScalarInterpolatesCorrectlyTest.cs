using FastGeoMesh.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    /// <summary>
    /// Tests for class LerpScalarInterpolatesCorrectlyTest.
    /// </summary>
    public sealed class LerpScalarInterpolatesCorrectlyTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var services = new ServiceCollection();
            services.AddFastGeoMesh();
            var provider = services.BuildServiceProvider();
            var helper = provider.GetRequiredService<IGeometryHelper>();

            helper.LerpScalar(0, 10, 0.0).Should().Be(0);
            helper.LerpScalar(0, 10, 1.0).Should().Be(10);
            helper.LerpScalar(0, 10, 0.5).Should().Be(5);
            helper.LerpScalar(5, 15, 0.25).Should().Be(7.5);
            helper.LerpScalar(-10, 10, 0.75).Should().Be(5);
        }
    }
}
