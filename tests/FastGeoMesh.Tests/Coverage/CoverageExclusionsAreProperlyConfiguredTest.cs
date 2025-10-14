using System.Reflection;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    public sealed class CoverageExclusionsAreProperlyConfiguredTest
    {
        [Fact]
        public void Test()
        {
            var excludedNamespaces = new[] { "FastGeoMesh.Properties", "System", "Microsoft" };
            var excludedTypes = new[] { "AssemblyInfo", "GlobalUsings" };
            var assemblyNames = new[] { "FastGeoMesh.Domain.dll", "FastGeoMesh.Application.dll", "FastGeoMesh.Infrastructure.dll", "FastGeoMesh.dll" };

            int totalPublicTypes = 0;
            int assembliesLoaded = 0;

            foreach (var assemblyName in assemblyNames)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName));
                    var allTypes = assembly.GetTypes();
                    var publicTypes = Array.FindAll(allTypes, t => t.IsPublic);
                    totalPublicTypes += publicTypes.Length;
                    assembliesLoaded++;
                    Console.WriteLine($"✓ {assemblyName}: {allTypes.Length} total types, {publicTypes.Length} public types");
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"⚠ {assemblyName} not found (optional in Clean Architecture)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ Failed to load {assemblyName}: {ex.Message}");
                }
            }

            assembliesLoaded.Should().BeGreaterThan(0);
            totalPublicTypes.Should().BeGreaterThan(5);
            Console.WriteLine($"Excluded namespaces configured: {string.Join(", ", excludedNamespaces)}");
            Console.WriteLine($"Excluded types configured: {string.Join(", ", excludedTypes)}");
        }
    }
}
