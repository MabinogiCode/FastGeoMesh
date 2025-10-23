// <copyright file="PerformanceMonitor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Diagnostics;

namespace FastGeoMesh.Utils
{
    /// <summary>
    /// Lightweight performance monitoring helper intended for debugging/profiling in development.
    /// This is a thin wrapper and does not replace the richer PerformanceMonitor in Infrastructure.
    /// </summary>
    internal static class PerformanceMonitor
    {
        public static Stopwatch Start()
        {
            var sw = new Stopwatch();
            sw.Start();
            return sw;
        }

        public static long StopAndGetElapsedMs(Stopwatch sw)
        {
            if (sw is null)
            {
                return 0;
            }

            sw.Stop();
            return sw.ElapsedMilliseconds;
        }
    }
}
