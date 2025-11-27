namespace FastGeoMesh.Tests.Helpers
{
    internal static class QuadHelpersCore
    {
        private const double Epsilon = 1e-9;
        /// <summary>
        /// Runs test IsQuadAtZ.
        /// </summary>
        public static bool IsQuadAtZ(FastGeoMesh.Domain.Quad q, double expectedZ)
        {
            return Math.Abs(q.V0.Z - expectedZ) < Epsilon &&
                   Math.Abs(q.V1.Z - expectedZ) < Epsilon &&
                   Math.Abs(q.V2.Z - expectedZ) < Epsilon &&
                   Math.Abs(q.V3.Z - expectedZ) < Epsilon;
        }
        /// <summary>
        /// Runs test IsCapQuad.
        /// </summary>
        public static bool IsCapQuad(FastGeoMesh.Domain.Quad q)
        {
            return Math.Abs(q.V0.Z - q.V1.Z) < Epsilon &&
                   Math.Abs(q.V1.Z - q.V2.Z) < Epsilon &&
                   Math.Abs(q.V2.Z - q.V3.Z) < Epsilon;
        }
    }
}
