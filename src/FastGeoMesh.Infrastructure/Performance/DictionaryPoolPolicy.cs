using Microsoft.Extensions.ObjectPool;

namespace FastGeoMesh.Infrastructure.Performance
{
    /// <summary>Enhanced pooled object policy for Dictionary&lt;K,V&gt; with memory optimization.</summary>
    internal sealed class DictionaryPoolPolicy<K, V> : PooledObjectPolicy<Dictionary<K, V>>
        where K : notnull
    {
        private const int MaxRetainedCount = 1024;

        public override Dictionary<K, V> Create()
        {
            return new Dictionary<K, V>();
        }

        public override bool Return(Dictionary<K, V> obj)
        {
            if (obj == null)
            {
                return false;
            }

            // Clear contents to prevent interference between uses
            obj.Clear();

            // Only retain dictionaries under a reasonable size threshold
            return obj.Count <= MaxRetainedCount;
        }
    }
}
