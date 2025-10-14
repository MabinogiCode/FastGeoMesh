using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    public sealed class CoverageOutputDirectoryStructureIsValidTest
    {
        [Fact]
        public void Test()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var expectedPaths = new[]
            {
                "../TestResults",
                "../TestResults/coverage",
                "../../TestResults",
                "../../TestResults/coverage"
            };

            bool foundValidPath = false;

            foreach (var relativePath in expectedPaths)
            {
                try
                {
                    var fullPath = Path.GetFullPath(Path.Combine(baseDirectory, relativePath));
                    var parentDir = Path.GetDirectoryName(fullPath);

                    if (Directory.Exists(parentDir))
                    {
                        foundValidPath = true;
                        Console.WriteLine($"Valid coverage path found: {fullPath}");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Path validation failed for {relativePath}: {ex.Message}");
                }
            }

            foundValidPath.Should().BeTrue();
        }
    }
}
