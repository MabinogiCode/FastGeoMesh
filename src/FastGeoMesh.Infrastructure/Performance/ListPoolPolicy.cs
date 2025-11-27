using Microsoft.Extensions.ObjectPool;

namespace FastGeoMesh.Infrastructure.Performance
{
    /// <summary>Enhanced pooled object policy for List&lt;T&gt; with configurable capacity limits.</summary>
    internal sealed class ListPoolPolicy<T> : PooledObjectPolicy<List<T>>
    {
        private readonly int _maxRetainedCapacity;

        public ListPoolPolicy(int maxRetainedCapacity = 1024)
        {
            _maxRetainedCapacity = maxRetainedCapacity;
        }

        public override List<T> Create()
        {
            return new List<T>();
        }

        public override bool Return(List<T> obj)
        {
            if (obj == null)
            {
                return false;
            }

            // Clear contents to prevent interference between uses
            obj.Clear();

            // Use configurable capacity limit for better memory management
            return obj.Capacity <= _maxRetainedCapacity;
        }
    }
}
