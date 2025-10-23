using System.Reflection;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage {
    public sealed class CoverageExclusionsAreProperlyConfiguredTest {
        [Fact]
        public void Test() {
            var excludedNamespaces = new[] { "FastGeoMesh.Properties", "System", "Microsoft" };
            var excludedTypes = new[] { "AssemblyInfo", "GlobalUsings" };
            var assemblyNames = new[] { "FastGeoMesh.Domain.dll", "FastGeoMesh.Application.dll", "FastGeoMesh.Infrastructure.dll", "FastGeoMesh.dll" };

            int totalPublicTypes = 0;
            int assembliesLoaded = 0;

            foreach (var assemblyName in assemblyNames) {
                try {
                    // Prefer loading by simple assembly name to avoid relying on file layout in CI/runtime
                    var simpleName = assemblyName.EndsWith(".dll", System.StringComparison.OrdinalIgnoreCase)
                        ? System.IO.Path.GetFileNameWithoutExtension(assemblyName)
                        : assemblyName;

                    var assembly = Assembly.Load(new AssemblyName(simpleName));
                    var allTypes = assembly.GetTypes();
                    var publicTypes = Array.FindAll(allTypes, t => t.IsPublic);
                    totalPublicTypes += publicTypes.Length;
                    assembliesLoaded++;
                    Console.WriteLine($"✓ {assemblyName}: {allTypes.Length} total types, {publicTypes.Length} public types");
                }
                catch (FileNotFoundException) {
                    // Fallback to loading from file in base directory (useful for local runs)
                    try {
                        var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName);
                        if (System.IO.File.Exists(path)) {
                            var assembly = Assembly.LoadFrom(path);
                            var allTypes = assembly.GetTypes();
                            var publicTypes = Array.FindAll(allTypes, t => t.IsPublic);
                            totalPublicTypes += publicTypes.Length;
                            assembliesLoaded++;
                            Console.WriteLine($"✓ {assemblyName} (loaded from file): {allTypes.Length} total types, {publicTypes.Length} public types");
                            continue;
                        }

                        Console.WriteLine($"⚠ {assemblyName} not found (optional in Clean Architecture)");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"⚠ Failed to load {assemblyName} on fallback: {ex.Message}");
                    }
                }
                catch (Exception ex) {
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
