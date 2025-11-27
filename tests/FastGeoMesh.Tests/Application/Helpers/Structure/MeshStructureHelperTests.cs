using FastGeoMesh.Application.Helpers.Structure;
using FastGeoMesh.Domain;
using FastGeoMesh.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Application.Helpers.Structures
{
    public class MeshStructureHelperTests
    {
        private readonly GeometryService _geometryService = new GeometryService();

        [Fact]
        public void BuildZLevelsWithNegativeTargetEdgeLengthDoesNotAddUniformLevels()
        {
            // Use the maximum allowed target length to effectively disable uniform subdivision
            var options = new MesherOptions { TargetEdgeLengthZ = EdgeLength.From(1e6) };
            var structure = new PrismStructureDefinition(new Polygon2D(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(0, 1) }), 0, 10);
            var levels = MeshStructureHelper.BuildZLevels(0, 10, options, structure);
            levels.Should().HaveCount(2); // Only z0 and z1
        }

        [Fact]
        public void BuildZLevelsWithZeroRangeDoesNotAddUniformLevels()
        {
            var options = new MesherOptions { TargetEdgeLengthZ = EdgeLength.From(1) };
            // Construct a valid structure (top > base), but query with z0==z1
            var structure = new PrismStructureDefinition(new Polygon2D(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(0, 1) }), 0, 10);
            var levels = MeshStructureHelper.BuildZLevels(0, 0, options, structure);
            levels.Should().HaveCount(1); // Only z0
        }

        [Fact]
        public void BuildZLevelsWithDuplicatesReturnsUniqueSortedLevels()
        {
            // Disable uniform levels to only test de-duplication of input-driven levels
            var options = new MesherOptions { TargetEdgeLengthZ = EdgeLength.From(1e6) };
            var structure = new PrismStructureDefinition(new Polygon2D(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(0, 1) }), 0, 10);
            structure.Geometry.AddPoint(new Vec3(0, 0, 5));
            structure.Geometry.AddPoint(new Vec3(0, 0, 5)); // Duplicate
            var levels = MeshStructureHelper.BuildZLevels(0, 10, options, structure);
            levels.Should().HaveCount(3); // 0, 5, 10
            levels.Should().BeInAscendingOrder();
        }

        [Fact]
        public void IsNearAnyHoleWithNoHolesReturnsFalse()
        {
            var structure = new PrismStructureDefinition(new Polygon2D(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(0, 1) }), 0, 10);
            MeshStructureHelper.IsNearAnyHole(structure, 0.5, 0.5, 0.1, _geometryService).Should().BeFalse();
        }

        [Fact]
        public void IsNearAnySegmentWithNoSegmentsReturnsFalse()
        {
            var structure = new PrismStructureDefinition(new Polygon2D(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(0, 1) }), 0, 10);
            MeshStructureHelper.IsNearAnySegment(structure, 0.5, 0.5, 0.1, _geometryService).Should().BeFalse();
        }

        [Fact]
        public void IsInsideAnyHoleWithNoHolesReturnsFalse()
        {
            var structure = new PrismStructureDefinition(new Polygon2D(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(0, 1) }), 0, 10);
            MeshStructureHelper.IsInsideAnyHole(structure, 0.5, 0.5, _geometryService).Should().BeFalse();
        }

        [Fact]
        public void IsInsideAnyHoleWithNoHoleIndicesReturnsFalse()
        {
            MeshStructureHelper.IsInsideAnyHole(Array.Empty<ISpatialPolygonIndex>(), 0.5, 0.5).Should().BeFalse();
        }
    }
}
