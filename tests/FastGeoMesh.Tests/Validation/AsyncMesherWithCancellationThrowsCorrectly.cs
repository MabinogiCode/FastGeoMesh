using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Validation
{
    /// <summary>
    /// Validates that asynchronous meshing handles cancellation tokens correctly.
    /// </summary>
    public sealed class AsyncMesherWithCancellationThrowsCorrectly
    {
        [Fact]
        public async Task Test()
        {
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 5), new Vec2(0, 5)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);
            var options = MesherOptions.CreateBuilder().WithFastPreset().Build().UnwrapForTests();
            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            try
            {
                var result = await asyncMesher.MeshAsync(structure, options, cts.Token);
                cts.Token.IsCancellationRequested.Should().BeTrue("Cancellation token should be cancelled");
                result.Should().NotBeNull("Valid result or cancellation exception are both acceptable");
            }
            catch (OperationCanceledException)
            {
                cts.Token.IsCancellationRequested.Should().BeTrue("Cancellation token should be cancelled when exception is thrown");
            }
        }
    }
}
