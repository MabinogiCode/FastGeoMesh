using FastGeoMesh.Domain;

namespace FastGeoMesh.Tests.Helpers
{
    /// <summary>
    /// Tests for class TestExtensions.
    /// </summary>
    public static class TestExtensions
    {
        /// <summary>
        /// Runs test UnwrapForTests.
        /// </summary>
        public static MesherOptions UnwrapForTests(this Result<MesherOptions> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Options validation failed: {result.Error.Description}");
            }

            return result.Value;
        }
        /// <summary>
        /// Runs test UnwrapForTests.
        /// </summary>
        public static ImmutableMesh UnwrapForTests(this Result<ImmutableMesh> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Meshing failed: {result.Error.Description}");
            }

            return result.Value;
        }
        /// <summary>
        /// Public API used by tests.
        /// </summary>
        public static async Task<ImmutableMesh> UnwrapForTestsAsync(this ValueTask<Result<ImmutableMesh>> result)
        {
            var r = await result.ConfigureAwait(true);
            if (r.IsFailure)
            {
                // Preserve cancellation semantics
                if (r.Error.Description.Contains("cancelled", StringComparison.OrdinalIgnoreCase) || r.Error.Description.Contains("canceled", StringComparison.OrdinalIgnoreCase))
                {
                    throw new OperationCanceledException(r.Error.Description);
                }
                throw new InvalidOperationException($"Async meshing failed: {r.Error.Description}");
            }
            return r.Value;
        }
        /// <summary>
        /// Public API used by tests.
        /// </summary>
        public static async Task<IReadOnlyList<ImmutableMesh>> UnwrapForTestsAsync(this ValueTask<Result<IReadOnlyList<ImmutableMesh>>> result)
        {
            var r = await result.ConfigureAwait(true);
            if (r.IsFailure)
            {
                throw new InvalidOperationException($"Async batch meshing failed: {r.Error.Description}");
            }

            return r.Value;
        }
    }
}
