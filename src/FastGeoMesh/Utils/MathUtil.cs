namespace FastGeoMesh.Utils;

public static class MathUtil
{
    /// <summary>
    /// Compares two doubles with a relative tolerance. Always allows absolute epsilon around zero.
    /// </summary>
    public static bool NearlyEqual(double a, double b, double eps)
    {
        if (double.IsNaN(a) || double.IsNaN(b)) return false;
        if (double.IsInfinity(a) || double.IsInfinity(b)) return a.Equals(b);
        double diff = Math.Abs(a - b);
        if (diff <= eps) return true; // absolute tolerance near zero
        double scale = Math.Max(1.0, Math.Max(Math.Abs(a), Math.Abs(b)));
        return diff <= eps * scale;
    }
}
