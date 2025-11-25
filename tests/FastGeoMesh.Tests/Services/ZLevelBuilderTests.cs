using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using Xunit;

namespace FastGeoMesh.Tests.Services
{
    public class ZLevelBuilderTests
    {
        private readonly ZLevelBuilder _builder;

        public ZLevelBuilderTests()
        {
            _builder = new ZLevelBuilder();
        }

        [Fact]
        public void BuildZLevelsReturnsBaseAndTopElevations()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0
            );

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthZ(5.0)
                .Build().Value;

            // Act
            var levels = _builder.BuildZLevels(0.0, 10.0, options, structure);

            // Assert
            Assert.NotNull(levels);
            Assert.Contains(0.0, levels);
            Assert.Contains(10.0, levels);
        }

        [Fact]
        public void BuildZLevelsCreatesUniformSubdivisions()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0
            );

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthZ(2.5)
                .Build().Value;

            // Act
            var levels = _builder.BuildZLevels(0.0, 10.0, options, structure);

            // Assert
            Assert.NotNull(levels);
            Assert.True(levels.Count >= 5); // At least 5 levels for 10.0 range with 2.5 target
        }

        [Fact]
        public void BuildZLevelsIncludesInternalSurfaceElevations()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0
            ).AddInternalSurface(
                new Polygon2D(new[] { new Vec2(2, 2), new Vec2(8, 2), new Vec2(8, 8), new Vec2(2, 8) }),
                z: 5.0
            );

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthZ(100.0) // Large value to avoid automatic subdivisions
                .Build().Value;

            // Act
            var levels = _builder.BuildZLevels(0.0, 10.0, options, structure);

            // Assert
            Assert.Contains(5.0, levels);
        }

        [Fact]
        public void BuildZLevelsReturnsDistinctSortedLevels()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(10, 10), new Vec2(0, 10) }),
                baseElevation: 0.0,
                topElevation: 10.0
            );

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthZ(2.0)
                .Build().Value;

            // Act
            var levels = _builder.BuildZLevels(0.0, 10.0, options, structure);

            // Assert
            Assert.NotNull(levels);
            Assert.Equal(levels.Distinct().Count(), levels.Count); // All levels are unique
            Assert.Equal(levels.OrderBy(z => z).ToArray(), levels.ToArray()); // Sorted ascending
        }

        [Fact]
        public void BuildZLevelsHandlesMinimalStructure()
        {
            // Arrange
            var structure = new PrismStructureDefinition(
                new Polygon2D(new[] { new Vec2(0, 0), new Vec2(1, 0), new Vec2(1, 1) }),
                baseElevation: 0.0,
                topElevation: 1.0
            );

            var options = MesherOptions.CreateBuilder().Build().Value;

            // Act
            var levels = _builder.BuildZLevels(0.0, 1.0, options, structure);

            // Assert
            Assert.NotNull(levels);
            Assert.True(levels.Count >= 2); // At least base and top
        }
    }
}
