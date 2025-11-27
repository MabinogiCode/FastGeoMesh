using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Domain
{
    /// <summary>
    /// Tests for class ImmutableStructureAddHoleReturnsNewInstanceTest.
    /// </summary>
    public sealed class ImmutableStructureAddHoleReturnsNewInstanceTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) });
            var baseStruct = new PrismStructureDefinition(outer, 0, 5);
            var hole = Polygon2D.FromPoints(new[] { new Vec2(1, 1), new Vec2(2, 1), new Vec2(2, 2), new Vec2(1, 2) });
            var next = baseStruct.AddHole(hole);
            next.Should().NotBeSameAs(baseStruct);
            baseStruct.Holes.Should().BeEmpty();
            next.Holes.Should().HaveCount(1);
        }
    }
}
