using FastGeoMesh.Domain;

namespace FastGeoMesh.Application.Helpers
{
    /// <summary>
    /// Helper for PrismMesher static methods (complexity estimation, etc).
    /// </summary>
    public static class PrismMesherHelper
    {
        /// <summary>
        /// Estimates the computational complexity for a given prism structure definition.
        /// </summary>
        /// <param name="structure">The prism structure definition.</param>
        /// <returns>The estimated meshing complexity.</returns>
        public static MeshingComplexity EstimateComplexity(PrismStructureDefinition structure)
        {
            var totalVertices = structure.Footprint.Count + structure.Holes.Sum(h => h.Count);
            return totalVertices switch
            {
                < 10 => MeshingComplexity.Trivial,
                < 50 => MeshingComplexity.Simple,
                < 200 => MeshingComplexity.Moderate,
                < 1000 => MeshingComplexity.Complex,
                _ => MeshingComplexity.Extreme
            };
        }

        /// <summary>
        /// Creates a detailed complexity estimate for a given prism structure and complexity.
        /// </summary>
        /// <param name="structure">The prism structure definition.</param>
        /// <param name="complexity">The estimated complexity.</param>
        /// <returns>A detailed complexity estimate.</returns>
        public static MeshingComplexityEstimate CreateDetailedEstimate(PrismStructureDefinition structure, MeshingComplexity complexity)
        {
            var footprintVertices = structure.Footprint.Count;
            var holeVertices = structure.Holes.Sum(h => h.Count);
            var totalVertices = footprintVertices + holeVertices;
            var estimatedQuads = (int)(totalVertices * 1.5 + structure.InternalSurfaces.Count * 10);
            var estimatedTriangles = System.Math.Max(1, (int)(totalVertices * 0.3));
            var estimatedMemory = (estimatedQuads + estimatedTriangles) * 160L;

            static System.TimeSpan FromMicroseconds(long microseconds) => System.TimeSpan.FromTicks(microseconds * 10);

            var estimatedTime = complexity switch
            {
                MeshingComplexity.Trivial => FromMicroseconds(80),
                MeshingComplexity.Simple => FromMicroseconds(240),
                MeshingComplexity.Moderate => FromMicroseconds(800),
                MeshingComplexity.Complex => System.TimeSpan.FromMilliseconds(4),
                MeshingComplexity.Extreme => System.TimeSpan.FromMilliseconds(16),
                _ => FromMicroseconds(800)
            };
            var recommendedParallelism = complexity >= MeshingComplexity.Complex ? System.Math.Min(System.Environment.ProcessorCount, 4) : 1;
            var hints = new System.Collections.Generic.List<string>();
            if (complexity >= MeshingComplexity.Complex)
            {
                hints.Add("Consider using parallel batch processing for multiple structures");
            }

            if (structure.Holes.Count > 5)
            {
                hints.Add("Large number of holes detected - consider hole refinement options");
            }

            if (totalVertices > 500)
            {
                hints.Add("Large geometry detected - async processing recommended");
            }

            if (complexity == MeshingComplexity.Trivial)
            {
                hints.Add("Simple geometry - synchronous processing is optimal");
            }

            if (complexity == MeshingComplexity.Moderate && structure.Holes.Count > 0)
            {
                hints.Add("Moderate complexity with holes - consider async processing for better performance");
            }

            if (complexity >= MeshingComplexity.Moderate && complexity < MeshingComplexity.Complex && totalVertices > 50)
            {
                hints.Add("Consider async processing for improved responsiveness");
            }

            return new MeshingComplexityEstimate(estimatedQuads, estimatedTriangles, estimatedMemory, estimatedTime, recommendedParallelism, complexity, hints);
        }
    }
}
