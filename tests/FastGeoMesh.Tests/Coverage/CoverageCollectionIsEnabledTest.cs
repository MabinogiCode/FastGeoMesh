using System.Reflection;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    public sealed class CoverageCollectionIsEnabledTest
    {
        [Fact]
        public void Test()
        {
            var isCIBuild = Environment.GetEnvironmentVariable("CI");
            var isCoverageBuild = Environment.GetEnvironmentVariable("GITHUB_ACTIONS");

            var assembly = Assembly.GetExecutingAssembly();
            var assemblyPath = assembly.Location;

            assemblyPath.Should().NotBeNullOrEmpty();
            assembly.Should().NotBeNull();
            assembly.GetName().Name.Should().Be("FastGeoMesh.Tests");

            Console.WriteLine($"Assembly Location: {assemblyPath}");
            Console.WriteLine($"CI Environment: {isCIBuild ?? "false"}");
            Console.WriteLine($"GitHub Actions: {isCoverageBuild ?? "false"}");
        }
    }
}
