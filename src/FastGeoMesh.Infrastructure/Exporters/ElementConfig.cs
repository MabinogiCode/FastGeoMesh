namespace FastGeoMesh.Infrastructure.Exporters
{
    /// <summary>Configuration for element formatting in TXT export.</summary>
    public sealed class ElementConfig
    {
        /// <summary>Prefix to add before each element line.</summary>
        public string Prefix { get; }

        /// <summary>Where to place the element count (top, bottom, or none).</summary>
        public CountPlacement CountPlacement { get; }

        /// <summary>Whether to include element index in the output.</summary>
        public bool IndexBased { get; }

        /// <summary>Create a new element configuration.</summary>
        /// <param name="prefix">Prefix to add before each element line.</param>
        /// <param name="countPlacement">Where to place the element count.</param>
        /// <param name="indexBased">Whether to include element index in the output.</param>
        public ElementConfig(string prefix, CountPlacement countPlacement, bool indexBased)
        {
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            CountPlacement = countPlacement;
            IndexBased = indexBased;
        }
    }
}
