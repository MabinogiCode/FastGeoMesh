namespace FastGeoMesh.Utils
{
    /// <summary>Math helper utilities.</summary>
    public static class MathUtil
    {
        /// <summary>
        /// Relative/absolute comparison of two doubles with tolerance eps.
        /// </summary>
        public static bool NearlyEqual(double a, double b, double eps)
        {
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }
            if (double.IsInfinity(a) || double.IsInfinity(b))
            {
                return a.Equals(b);
            }
            double diff = Math.Abs(a - b);
            if (diff <= eps)
            {
                return true;
            }
            double scale = Math.Max(1.0, Math.Max(Math.Abs(a), Math.Abs(b)));
            return diff <= eps * scale;
        }
    }
}
