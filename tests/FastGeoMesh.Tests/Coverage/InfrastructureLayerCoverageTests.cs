using FastGeoMesh.Application.Services;
using FastGeoMesh.Domain;
using FastGeoMesh.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    /// <summary>
    /// Additional Infrastructure layer coverage tests.
    /// Focuses on exporters, performance monitoring, and utility classes.
    /// </summary>
    public sealed class InfrastructureLayerCoverageTests
    {
        /// <summary>Tests mesh validation and helper operations.</summary>
        [Fact]
        public void MeshValidationAndHelperOperationsWorkCorrectly()
        {
            // Create test mesh
            var mesh = new ImmutableMesh();
            mesh = mesh.AddQuad(new Quad(
                new Vec3(0, 0, 0), new Vec3(1, 0, 0),
                new Vec3(1, 1, 0), new Vec3(0, 1, 0)));
            mesh = mesh.AddTriangle(new Triangle(
                new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2.5, 1, 0)));
            mesh = mesh.AddPoint(new Vec3(5, 5, 5));
            mesh = mesh.AddInternalSegment(new Segment3D(
                new Vec3(0, 0, 1), new Vec3(1, 1, 1)));

            // Test mesh properties
            mesh.QuadCount.Should().Be(1);
            mesh.TriangleCount.Should().Be(1);
            mesh.Points.Should().HaveCount(1);
            mesh.InternalSegments.Should().HaveCount(1);

            // Test with indexed mesh
            var indexed = IndexedMesh.FromMesh(mesh);
            indexed.VertexCount.Should().BeGreaterThan(0);
            indexed.QuadCount.Should().Be(1);
            indexed.TriangleCount.Should().Be(1);
            indexed.EdgeCount.Should().BeGreaterThan(0);

            // Test mesh adjacency
            var adjacency = indexed.BuildAdjacency();
            adjacency.QuadCount.Should().Be(1);
            adjacency.Neighbors.Should().HaveCount(1);
            adjacency.BoundaryEdges.Should().NotBeEmpty();
            adjacency.NonManifoldEdges.Should().BeEmpty();
        }

        /// <summary>Tests async meshing operations and progress reporting.</summary>
        [Fact]
        public async Task AsyncMeshingOperationsAndProgressReportingWorkCorrectly()
        {
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 3), new Vec2(0, 3)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 2);

            var options = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .Build().UnwrapForTests();

            var mesher = new PrismMesher();
            var asyncMesher = (IAsyncMesher)mesher;

            // Test basic async meshing
            var result = await asyncMesher.MeshAsync(structure, options);
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            // Test complexity estimation
            var estimate = await asyncMesher.EstimateComplexityAsync(structure, options);
            estimate.Should().NotBeNull();
            estimate.EstimatedQuadCount.Should().BeGreaterThan(0);
            estimate.EstimatedTriangleCount.Should().BeGreaterThanOrEqualTo(0);
            estimate.EstimatedComputationTime.Should().BeGreaterThan(TimeSpan.Zero);
            estimate.RecommendedParallelism.Should().BeGreaterThan(0);

            // Test progress reporting
            var progressReports = new List<MeshingProgress>();
            var progress = new Progress<MeshingProgress>(p => progressReports.Add(p));

            var resultWithProgress = await asyncMesher.MeshWithProgressAsync(
                structure, options, progress);
            resultWithProgress.IsSuccess.Should().BeTrue();

            // Progress should be reported (at least start and end)
            progressReports.Should().NotBeEmpty();

            // Test batch operations
            var structures = new[]
            {
                structure,
                new PrismStructureDefinition(polygon, 2, 4),
                new PrismStructureDefinition(polygon, 4, 6)
            };

            var batchResult = await asyncMesher.MeshBatchAsync(structures, options);
            batchResult.IsSuccess.Should().BeTrue();
            batchResult.Value.Should().HaveCount(3);

            // Test performance statistics
            var stats = await asyncMesher.GetLivePerformanceStatsAsync();
            stats.Should().NotBeNull();
            stats.MeshingOperations.Should().BeGreaterThanOrEqualTo(0);
        }

        /// <summary>Tests various meshing scenarios with different geometries.</summary>
        [Fact]
        public void VariousMeshingScenariosWithDifferentGeometriesWorkCorrectly()
        {
            var mesher = new PrismMesher();

            // Test simple rectangle
            var rect = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(4, 0), new Vec2(4, 2), new Vec2(0, 2)
            });
            var rectStructure = new PrismStructureDefinition(rect, 0, 1);

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.5)
                .WithTargetEdgeLengthZ(0.5)
                .WithCaps(bottom: true, top: true)
                .Build().UnwrapForTests();

            var rectResult = mesher.Mesh(rectStructure, options);
            rectResult.IsSuccess.Should().BeTrue();
            var rectMesh = rectResult.Value;
            rectMesh.QuadCount.Should().BeGreaterThan(0);

            // Test L-shaped polygon
            var lShape = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(3, 0), new Vec2(3, 1),
                new Vec2(1, 1), new Vec2(1, 3), new Vec2(0, 3)
            });
            var lStructure = new PrismStructureDefinition(lShape, 0, 1);

            var lResult = mesher.Mesh(lStructure, options);
            lResult.IsSuccess.Should().BeTrue();
            var lMesh = lResult.Value;

            // L-shape should have quads and/or triangles
            var totalElements = lMesh.QuadCount + lMesh.TriangleCount;
            totalElements.Should().BeGreaterThan(0);

            // Test polygon with hole
            var outerWithHole = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(5, 0), new Vec2(5, 5), new Vec2(0, 5)
            });
            var hole = Polygon2D.FromPoints(new[]
            {
                new Vec2(1, 1), new Vec2(4, 1), new Vec2(4, 4), new Vec2(1, 4)
            });
            var holeStructure = new PrismStructureDefinition(outerWithHole, 0, 1)
                .AddHole(hole);

            var holeResult = mesher.Mesh(holeStructure, options);
            holeResult.IsSuccess.Should().BeTrue();
            var holeMesh = holeResult.Value;

            var holeElements = holeMesh.QuadCount + holeMesh.TriangleCount;
            holeElements.Should().BeGreaterThan(0);

            // Test with auxiliary geometry
            var auxStructure = new PrismStructureDefinition(rect, 0, 2);
            auxStructure.Geometry.AddPoint(new Vec3(2, 1, 1));
            auxStructure.Geometry.AddSegment(new Segment3D(
                new Vec3(0, 1, 1), new Vec3(4, 1, 1)));

            var auxResult = mesher.Mesh(auxStructure, options);
            auxResult.IsSuccess.Should().BeTrue();
            var auxMesh = auxResult.Value;

            auxMesh.Points.Should().HaveCount(1);
            auxMesh.InternalSegments.Should().HaveCount(1);
        }

        /// <summary>Tests different meshing options and presets.</summary>
        [Fact]
        public void DifferentMeshingOptionsAndPresetsWorkCorrectly()
        {
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(3, 0), new Vec2(3, 2), new Vec2(0, 2)
            });
            var structure = new PrismStructureDefinition(polygon, 0, 1);
            var mesher = new PrismMesher();

            // Test fast preset
            var fastOptions = MesherOptions.CreateBuilder()
                .WithFastPreset()
                .Build().UnwrapForTests();

            var fastResult = mesher.Mesh(structure, fastOptions);
            fastResult.IsSuccess.Should().BeTrue();

            // Test high quality preset
            var qualityOptions = MesherOptions.CreateBuilder()
                .WithHighQualityPreset()
                .Build().UnwrapForTests();

            var qualityResult = mesher.Mesh(structure, qualityOptions);
            qualityResult.IsSuccess.Should().BeTrue();

            // Test custom options
            var customOptions = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.3)
                .WithTargetEdgeLengthZ(0.2)
                .WithCaps(bottom: true, top: true)
                .WithMinCapQuadQuality(0.8)
                .WithRejectedCapTriangles(false)
                .Build().UnwrapForTests();

            var customResult = mesher.Mesh(structure, customOptions);
            customResult.IsSuccess.Should().BeTrue();

            // Test caps only bottom
            var bottomOnlyOptions = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.5)
                .WithTargetEdgeLengthZ(0.5)
                .WithCaps(bottom: true, top: false)
                .Build().UnwrapForTests();

            var bottomOnlyResult = mesher.Mesh(structure, bottomOnlyOptions);
            bottomOnlyResult.IsSuccess.Should().BeTrue();
            var bottomOnlyMesh = bottomOnlyResult.Value;

            // Should have bottom cap elements
            var bottomElements = bottomOnlyMesh.Quads.Where(q =>
                Math.Abs(q.V0.Z - 0) < 1e-10 &&
                Math.Abs(q.V1.Z - 0) < 1e-10 &&
                Math.Abs(q.V2.Z - 0) < 1e-10 &&
                Math.Abs(q.V3.Z - 0) < 1e-10).ToList();
            var bottomTriangles = bottomOnlyMesh.Triangles.Where(t =>
                Math.Abs(t.V0.Z - 0) < 1e-10 &&
                Math.Abs(t.V1.Z - 0) < 1e-10 &&
                Math.Abs(t.V2.Z - 0) < 1e-10).ToList();

            var totalBottomElements = bottomElements.Count + bottomTriangles.Count;
            totalBottomElements.Should().BeGreaterThan(0);
        }

        /// <summary>Tests mesh export functionality if available.</summary>
        [Fact]
        public void MeshExportFunctionalityIfAvailableWorksCorrectly()
        {
            // Create simple test mesh
            var mesh = new ImmutableMesh();
            mesh = mesh.AddQuad(new Quad(
                new Vec3(0, 0, 0), new Vec3(1, 0, 0),
                new Vec3(1, 1, 0), new Vec3(0, 1, 0)));

            var indexed = IndexedMesh.FromMesh(mesh);

            // Test basic mesh properties for export
            indexed.Vertices.Should().NotBeEmpty();
            indexed.Quads.Should().HaveCount(1);
            indexed.Edges.Should().NotBeEmpty();

            // Test that mesh can be converted to different representations
            var vertices = indexed.Vertices.ToList();
            vertices.Should().HaveCount(4);

            var quads = indexed.Quads.ToList();
            quads.Should().HaveCount(1);

            var edges = indexed.Edges.ToList();
            edges.Should().NotBeEmpty();

            // Test mesh validation for export
            foreach (var quad in quads)
            {
                quad.Item1.Should().BeInRange(0, vertices.Count - 1);
                quad.Item2.Should().BeInRange(0, vertices.Count - 1);
                quad.Item3.Should().BeInRange(0, vertices.Count - 1);
                quad.Item4.Should().BeInRange(0, vertices.Count - 1);
            }

            foreach (var edge in edges)
            {
                edge.a.Should().BeInRange(0, vertices.Count - 1);
                edge.b.Should().BeInRange(0, vertices.Count - 1);
                edge.a.Should().NotBe(edge.b);
            }
        }

        /// <summary>Tests geometric utility operations.</summary>
        [Fact]
        public void GeometricUtilityOperationsWorkCorrectly()
        {
            // Test Vec2 operations
            var v2a = new Vec2(3, 4);
            var v2b = new Vec2(1, 2);

            var sum = v2a + v2b;
            sum.Should().Be(new Vec2(4, 6));

            var diff = v2a - v2b;
            diff.Should().Be(new Vec2(2, 2));

            var scaled = v2a * 2.0;
            scaled.Should().Be(new Vec2(6, 8));

            var length = v2a.Length();
            length.Should().BeApproximately(5.0, 1e-10);

            var normalized = v2a.Normalize();
            normalized.Length().Should().BeApproximately(1.0, 1e-10);

            // Test Vec3 operations
            var v3a = new Vec3(1, 2, 3);
            var v3b = new Vec3(4, 5, 6);

            var sum3 = v3a + v3b;
            sum3.Should().Be(new Vec3(5, 7, 9));

            var cross = v3a.Cross(v3b);
            cross.Should().Be(new Vec3(-3, 6, -3));

            var dot = v3a.Dot(v3b);
            dot.Should().Be(32); // 1*4 + 2*5 + 3*6

            var length3 = v3a.Length();
            length3.Should().BeApproximately(Math.Sqrt(14), 1e-10);

            // Test Segment operations
            var segment2D = new Segment2D(new Vec2(0, 0), new Vec2(3, 4));
            var segmentLength = segment2D.Length();
            segmentLength.Should().BeApproximately(5.0, 1e-10);

            var segment3D = new Segment3D(new Vec3(0, 0, 0), new Vec3(3, 4, 0));
            segment3D.Start.Should().Be(new Vec3(0, 0, 0));
            segment3D.End.Should().Be(new Vec3(3, 4, 0));
        }

        /// <summary>Tests complex scenarios with constraints and internal surfaces.</summary>
        [Fact]
        public void ComplexScenariosWithConstraintsAndInternalSurfacesWorkCorrectly()
        {
            var polygon = Polygon2D.FromPoints(new[]
            {
                new Vec2(0, 0), new Vec2(6, 0), new Vec2(6, 4), new Vec2(0, 4)
            });

            var structure = new PrismStructureDefinition(polygon, 0, 3);

            // Add constraint segments
            var horizontalConstraint = new Segment2D(new Vec2(1, 2), new Vec2(5, 2));
            var verticalConstraint = new Segment2D(new Vec2(3, 0), new Vec2(3, 4));

            structure = structure
                .AddConstraintSegment(horizontalConstraint, 1.5)
                .AddConstraintSegment(verticalConstraint, 2.0);

            // Add internal surface
            var internalSurface = Polygon2D.FromPoints(new[]
            {
                new Vec2(1, 1), new Vec2(5, 1), new Vec2(5, 3), new Vec2(1, 3)
            });

            structure = structure.AddInternalSurface(internalSurface, 1.0);

            var options = MesherOptions.CreateBuilder()
                .WithTargetEdgeLengthXY(0.5)
                .WithTargetEdgeLengthZ(0.5)
                .WithCaps(bottom: true, top: true)
                .Build().UnwrapForTests();

            var mesher = new PrismMesher();
            var result = mesher.Mesh(structure, options);

            result.IsSuccess.Should().BeTrue();
            var mesh = result.Value;

            // Should have mesh elements at different Z levels
            var zValues = mesh.Quads
                .SelectMany(q => new[] { q.V0.Z, q.V1.Z, q.V2.Z, q.V3.Z })
                .Concat(mesh.Triangles.SelectMany(t => new[] { t.V0.Z, t.V1.Z, t.V2.Z }))
                .Distinct()
                .OrderBy(z => z)
                .ToList();

            zValues.Should().Contain(0.0); // Bottom
            zValues.Should().Contain(3.0); // Top
            zValues.Should().Contain(z => Math.Abs(z - 1.0) < 0.1); // Internal surface
            zValues.Should().Contain(z => Math.Abs(z - 1.5) < 0.1); // Constraint
            zValues.Should().Contain(z => Math.Abs(z - 2.0) < 0.1); // Constraint

            // Verify constraint segments and internal surfaces are present
            structure.ConstraintSegments.Should().HaveCount(2);
            structure.InternalSurfaces.Should().HaveCount(1);
        }
    }
}

