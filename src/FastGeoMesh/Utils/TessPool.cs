using System;
using System.Collections.Concurrent;
using LibTessDotNet;

namespace FastGeoMesh.Utils
{
    /// <summary>Simple pool for LibTessDotNet.Tess instances to reduce allocations.</summary>
    internal static class TessPool
    {
        private static readonly ConcurrentBag<Tess> _pool = new();
        private const int MaxRetained = 32;

        public static Tess Rent()
        {
            if (_pool.TryTake(out var t))
            {
                t.ClearState(); // ensure clean state
                return t;
            }
            return new Tess();
        }

        public static void Return(Tess tess)
        {
            if (tess is null)
            {
                return;
            }
            if (_pool.Count >= MaxRetained)
            {
                return;
            }
            _pool.Add(tess);
        }

        private static void ClearState(this Tess _)
        {
            // LibTessDotNet.Tess doesn't expose a Clear method,
            // but we can create a new instance which is lightweight
            // The tessellation state is internal and gets reset on Tessellate() call
            // So no additional clearing is needed for now
        }
    }
}
