using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure;
using FastGeoMesh.Infrastructure.Performance;
using FastGeoMesh.Infrastructure.Services;
using FluentAssertions;
using LibTessDotNet;
using Xunit;

namespace FastGeoMesh.Tests.Coverage {
    public sealed class InfrastructureUtilitiesCoverageTests {
        [Fact]
        public void SystemClockProvidesNowAndUtcNow() {
            var clock = new SystemClock();
            var before = DateTime.UtcNow;
            var utc = clock.UtcNow;
            var after = DateTime.UtcNow;
            utc.Should().BeOnOrAfter(before);
            utc.Should().BeOnOrBefore(after);

            var beforeLocal = DateTime.Now;
            var local = clock.Now;
            var afterLocal = DateTime.Now;
            local.Should().BeOnOrAfter(beforeLocal);
            local.Should().BeOnOrBefore(afterLocal);
        }

        [Fact]
        public void MeshPoolWithPooledMeshExecutesActionAndFunc() {
            bool actionCalled = false;
            PooledMeshExtensions.WithPooledMesh((ImmutableMesh m) => { actionCalled = true; });
            actionCalled.Should().BeTrue();

            var result = PooledMeshExtensions.WithPooledMesh((ImmutableMesh m) => 123);
            result.Should().Be(123);
        }

        [Fact]
        public void OptimizedConstantsLookupsReturnExpectedResults() {
            // Known valid category
            OptimizedConstants.IsValidEdgeLength("XY", 1e-3).Should().BeTrue();
            // Out of range
            OptimizedConstants.IsValidEdgeLength("XY", 1e-9).Should().BeFalse();
            // Unknown category
            OptimizedConstants.IsValidEdgeLength("UNKNOWN", 1.0).Should().BeFalse();

            OptimizedConstants.MeetsQualityThreshold("MinCapQuad", 0.5).Should().BeTrue();
            OptimizedConstants.MeetsQualityThreshold("MinCapQuad", 0.1).Should().BeFalse();
            OptimizedConstants.MeetsQualityThreshold("NONEXISTENT", 1.0).Should().BeFalse();
        }

        [Fact]
        public void TessPoolRentReturnDoesNotThrow() {
            var tess = TessPool.Rent();
            tess.Should().NotBeNull();
            // Perform a minimal operation if possible
            try {
                TessPool.Return(tess);
            }
            catch (Exception ex) {
                // Should not throw
                Assert.True(false, ex.ToString());
            }
        }

        [Fact]
        public void MeshingPoolsListsAreClearedOnReturn() {
            var l = MeshingPools.IntListPool.Get();
            l.Add(1);
            l.Add(2);
            MeshingPools.IntListPool.Return(l);

            var reused = MeshingPools.IntListPool.Get();
            reused.Should().NotBeNull();
            reused.Count.Should().Be(0);
            MeshingPools.IntListPool.Return(reused);
        }
    }
}
