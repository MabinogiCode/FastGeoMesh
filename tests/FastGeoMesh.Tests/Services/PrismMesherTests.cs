using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Domain.Services;
using FastGeoMesh.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Services
{
    public class PrismMesherTests
    {
        private readonly GeometryService _geometryService;
        private readonly StubZLevelBuilder _zLevelBuilder;
        private readonly StubProximityChecker _proximityChecker;
        private readonly PrismMesher _mesher;

        private readonly PrismStructureDefinition _trivialStructure;

        public PrismMesherTests()
        {
            _geometryService = new GeometryService();
            _zLevelBuilder = new StubZLevelBuilder();
            _proximityChecker = new StubProximityChecker();

            _mesher = new PrismMesher(_geometryService, _zLevelBuilder, _proximityChecker);

            var footprint = Polygon2D.FromUnsafe(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1), new Vec2(0, 1) });
            _trivialStructure = new PrismStructureDefinition(footprint, 0, 10);
        }

        [Fact]
        public async Task MeshAsyncWithInvalidOptionsReturnsValidationError()
        {
            var options = new MesherOptions { MinCapQuadQuality = -1 };
            var result = await _mesher.MeshAsync(_trivialStructure, options);
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Meshing.ValidationError");
        }

        [Fact]
        public async Task MeshWithProgressAsyncWithInvalidOptionsReturnsValidationError()
        {
            var options = new MesherOptions { MinCapQuadQuality = 2 };
            var result = await _mesher.MeshWithProgressAsync(_trivialStructure, options, null);
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Meshing.ValidationError");
        }

        [Fact]
        public async Task MeshBatchAsyncWithInvalidOptionsReturnsValidationError()
        {
            var options = new MesherOptions { MinCapQuadQuality = double.NaN };
            var result = await _mesher.MeshBatchAsync(new[] { _trivialStructure }, options);
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Meshing.ValidationError");
        }

        [Fact]
        public async Task MeshBatchAsyncWithEmptyListReturnsFailure()
        {
            var result = await _mesher.MeshBatchAsync(new List<PrismStructureDefinition>(), new MesherOptions());
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Meshing.EmptyBatch");
        }

        [Fact]
        public async Task MeshAsyncWhenCancelledReturnsCancelledError()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var result = await _mesher.MeshAsync(_trivialStructure, new MesherOptions(), cts.Token);
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Meshing.Cancelled");
        }

        [Fact]
        public async Task MeshAsyncWithComplexStructureRunsOnTaskAndCanBeCancelled()
        {
            var complexFootprint = Polygon2D.FromUnsafe(Enumerable.Range(0, 1000).Select(i => new Vec2(Math.Cos(i * 2 * Math.PI / 1000), Math.Sin(i * 2 * Math.PI / 1000))));
            var complexStructure = new PrismStructureDefinition(complexFootprint, 0, 1);
            using var cts = new CancellationTokenSource();

            _zLevelBuilder.BuildZLevelsFunc = (z0, z1, opt, struc) =>
            {
                cts.Cancel();
                cts.Token.ThrowIfCancellationRequested();
                return new List<double>();
            };

            var result = await _mesher.MeshAsync(complexStructure, new MesherOptions(), cts.Token);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Meshing.Cancelled");
        }

        [Fact]
        public async Task MeshWithProgressAsyncWhenCancelledReturnsCancelledError()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var result = await _mesher.MeshWithProgressAsync(_trivialStructure, new MesherOptions(), null, cts.Token);
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Meshing.Cancelled");
        }

        [Fact]
        public async Task MeshBatchAsyncWhenCancelledReturnsCancelledError()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var result = await _mesher.MeshBatchAsync(new[] { _trivialStructure }, new MesherOptions(), progress: null, cancellationToken: cts.Token);
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Meshing.Cancelled");
        }

        [Fact]
        public async Task MeshBatchAsyncWithSingleItemCallsMeshWithProgress()
        {
            var result = await _mesher.MeshBatchAsync(new[] { _trivialStructure }, new MesherOptions());
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetLivePerformanceStatsAsyncReturnsStatsFromMonitor()
        {
            var stats = await _mesher.GetLivePerformanceStatsAsync();
            stats.Should().NotBeNull();
        }

        [Fact]
        public async Task EstimateComplexityAsyncReturnsEstimate()
        {
            var estimate = await _mesher.EstimateComplexityAsync(_trivialStructure, new MesherOptions());
            estimate.Should().NotBeNull();
            estimate.Complexity.Should().Be(MeshingComplexity.Trivial);
        }

        [Theory]
        [InlineData("ArgumentException")]
        [InlineData("InvalidOperationException")]
        [InlineData("ArithmeticException")]
        [InlineData("IndexOutOfRangeException")]
        [InlineData("Exception")]
        public void MeshCatchesAndWrapsExceptions(string exceptionType)
        {
            _zLevelBuilder.BuildZLevelsFunc = (z0, z1, opt, struc) => throw CreateException(exceptionType, "Test Error");

            var result = _mesher.Mesh(_trivialStructure, new MesherOptions());

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Contain("Error");
        }

        [Theory]
        [InlineData("ArgumentException")]
        [InlineData("InvalidOperationException")]
        [InlineData("ArithmeticException")]
        [InlineData("IndexOutOfRangeException")]
        [InlineData("Exception")]
        public async Task MeshAsyncCatchesAndWrapsExceptions(string exceptionType)
        {
            _zLevelBuilder.BuildZLevelsFunc = (z0, z1, opt, struc) => throw CreateException(exceptionType, "Test Error");

            var result = await _mesher.MeshAsync(_trivialStructure, new MesherOptions());

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Contain("Error");
        }

        [Theory]
        [InlineData("ArgumentException")]
        [InlineData("InvalidOperationException")]
        [InlineData("ArithmeticException")]
        [InlineData("IndexOutOfRangeException")]
        [InlineData("Exception")]
        public async Task MeshWithProgressAsyncCatchesAndWrapsExceptions(string exceptionType)
        {
            _zLevelBuilder.BuildZLevelsFunc = (z0, z1, opt, struc) => throw CreateException(exceptionType, "Test Error");

            var result = await _mesher.MeshWithProgressAsync(_trivialStructure, new MesherOptions(), null);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Contain("Error");
        }

        [Theory]
        [InlineData("ArgumentException")]
        [InlineData("InvalidOperationException")]
        [InlineData("ArithmeticException")]
        [InlineData("IndexOutOfRangeException")]
        [InlineData("Exception")]
        public async Task MeshBatchAsyncCatchesAndWrapsExceptions(string exceptionType)
        {
            _zLevelBuilder.BuildZLevelsFunc = (z0, z1, opt, struc) => throw CreateException(exceptionType, "Test Error");

            var result = await _mesher.MeshBatchAsync(new[] { _trivialStructure, _trivialStructure }, new MesherOptions());

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Contain("Error");
        }

        private static Exception CreateException(string type, string message)
        {
            return type switch
            {
                "ArgumentException" => new ArgumentException(message),
                "InvalidOperationException" => new InvalidOperationException(message),
                "ArithmeticException" => new ArithmeticException(message),
                "IndexOutOfRangeException" => new IndexOutOfRangeException(message),
                _ => new Exception(message),
            };
        }
    }
}
