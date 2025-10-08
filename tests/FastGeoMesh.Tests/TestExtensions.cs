using FastGeoMesh.Domain;

namespace FastGeoMesh.Tests
{
    /// <summary>
    /// V2.0 extensions to help tests transition from old API to new Result pattern.
    /// </summary>
    public static class TestExtensions
    {
        /// <summary>Extension to unwrap Result&lt;MesherOptions&gt; for tests expecting direct MesherOptions</summary>
        public static MesherOptions UnwrapForTests(this Result<MesherOptions> result)
        {
            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Options validation failed: {result.Error.Description}");
            }

            return result.Value;
        }

        /// <summary>Extension to unwrap Result&lt;ImmutableMesh&gt; for tests expecting direct ImmutableMesh</summary>
        public static ImmutableMesh UnwrapForTests(this Result<ImmutableMesh> result)
        {
            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Meshing failed: {result.Error.Description}");
            }

            return result.Value;
        }

        /// <summary>Extension to unwrap async Result&lt;ImmutableMesh&gt; for tests</summary>
        public static async Task<ImmutableMesh> UnwrapForTestsAsync(this ValueTask<Result<ImmutableMesh>> result)
        {
            var r = await result;
            if (r.IsFailure)
            {
                // ✅ Préserver l'OperationCanceledException
                if (r.Error.Description.Contains("cancelled") || r.Error.Description.Contains("canceled"))
                {
                    throw new OperationCanceledException(r.Error.Description);
                }
                throw new InvalidOperationException($"Async meshing failed: {r.Error.Description}");
            }
            return r.Value;
        }

        /// <summary>Extension to unwrap async Result&lt;IReadOnlyList&lt;ImmutableMesh&gt;&gt; for tests</summary>
        public static async Task<IReadOnlyList<ImmutableMesh>> UnwrapForTestsAsync(this ValueTask<Result<IReadOnlyList<ImmutableMesh>>> result)
        {
            var r = await result;
            if (r.IsFailure)
            {
                throw new InvalidOperationException($"Async batch meshing failed: {r.Error.Description}");
            }

            return r.Value;
        }
    }
}
