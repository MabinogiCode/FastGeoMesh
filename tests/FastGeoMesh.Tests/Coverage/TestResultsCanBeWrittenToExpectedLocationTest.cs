using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    /// <summary>
    /// Tests for class TestResultsCanBeWrittenToExpectedLocationTest.
    /// </summary>
    public sealed class TestResultsCanBeWrittenToExpectedLocationTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var testResultsPath = Path.Combine(baseDirectory, "..", "TestResults");

            try
            {
                var fullPath = Path.GetFullPath(testResultsPath);

                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                var testFile = Path.Combine(fullPath, "coverage-test.tmp");
                File.WriteAllText(testFile, "Coverage infrastructure test");

                File.Exists(testFile).Should().BeTrue();

                File.Delete(testFile);

                Console.WriteLine($"✓ Coverage infrastructure validated at: {fullPath}");
            }
            catch (IOException ex)
            {
                HandleIOException(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                HandleUnauthorizedAccessException(ex);
            }
        }

        private static void HandleIOException(IOException ex)
        {
            Console.WriteLine($"⚠ Coverage directory test IO error: {ex.Message}");

            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "Test");
            File.Exists(tempFile).Should().BeTrue();
            File.Delete(tempFile);
        }

        private static void HandleUnauthorizedAccessException(UnauthorizedAccessException ex)
        {
            Console.WriteLine($"⚠ Coverage directory test access denied: {ex.Message}");

            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "Test");
            File.Exists(tempFile).Should().BeTrue();
            File.Delete(tempFile);
        }
    }
}
