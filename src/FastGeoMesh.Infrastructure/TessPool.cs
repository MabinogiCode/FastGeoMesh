using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using LibTessDotNet;

namespace FastGeoMesh.Utils
{
    /// <summary>High-performance pool for LibTessDotNet.Tess instances with enhanced resource management.</summary>
    public static class TessPool
    {
        private static readonly ConcurrentBag<Tess> _pool = new();
        private const int MaxRetained = 32;
        private static volatile bool _isShuttingDown;

        /// <summary>Rent a Tess instance from the pool or create a new one.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Tess Rent()
        {
            if (!_isShuttingDown && _pool.TryTake(out var t))
            {
                t.ClearState(); // ensure clean state
                return t;
            }
            return new Tess();
        }

        /// <summary>Return a Tess instance to the pool for reuse.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(Tess tess)
        {
            if (tess is null || _isShuttingDown)
            {
                // Dispose if shutting down
                if (tess is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                return;
            }

            if (_pool.Count >= MaxRetained)
            {
                // Dispose excess items to prevent memory leaks
                if (tess is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                return;
            }
            _pool.Add(tess);
        }

        /// <summary>Clear and dispose all pooled items. Call during application shutdown.</summary>
        public static void Clear()
        {
            _isShuttingDown = true;

            while (_pool.TryTake(out var tess))
            {
                if (tess is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        /// <summary>Get current pool statistics for monitoring.</summary>
        public static (int PooledCount, bool IsShuttingDown) GetStatistics()
        {
            return (_pool.Count, _isShuttingDown);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ClearState(this Tess _)
        {
            // LibTessDotNet.Tess doesn't expose a Clear method,
            // but we can create a new instance which is lightweight
            // The tessellation state is internal and gets reset on Tessellate() call
            // So no additional clearing is needed for now
        }
    }
}
