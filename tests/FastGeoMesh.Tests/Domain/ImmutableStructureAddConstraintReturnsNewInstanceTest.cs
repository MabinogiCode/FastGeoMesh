using FastGeoMesh.Domain;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Domain
{
    /// <summary>
    /// Tests for class ImmutableStructureAddConstraintReturnsNewInstanceTest.
    /// </summary>
    public sealed class ImmutableStructureAddConstraintReturnsNewInstanceTest
    {
        /// <summary>
        /// Runs test Test.
        /// </summary>
        [Fact]
        public void Test()
        {
            var outer = Polygon2D.FromPoints(new[] { new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5) });
            var baseStruct = new PrismStructureDefinition(outer, 0, 5);
            var seg = new Segment2D(new Vec2(1, 1), new Vec2(4, 1));
            var next = baseStruct.AddConstraintSegment(seg, 2.5);
            next.Should().NotBeSameAs(baseStruct);
            baseStruct.ConstraintSegments.Should().BeEmpty();
            next.ConstraintSegments.Should().HaveCount(1);
        }
    }
}
