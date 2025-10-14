using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Geometry
{
    public sealed class FromMeshUsesExactKeyingWhenEpsilonIsDoubleEpsilonTest
    {
        [Fact]
        public void Test()
        {
            var p0 = new Vec3(0, 0, 0);
            var p1 = new Vec3(1e-10, 0, 0);
            var mesh = new ImmutableMesh()
                .AddPoint(p0)
                .AddPoint(p1);

            var imExact = IndexedMesh.FromMesh(mesh, double.Epsilon);
            _ = imExact.Vertices.Count.Should().Be(2);

            var imMerge = IndexedMesh.FromMesh(mesh, 1e-6);
            _ = imMerge.Vertices.Count.Should().Be(1);
        }
    }
}
