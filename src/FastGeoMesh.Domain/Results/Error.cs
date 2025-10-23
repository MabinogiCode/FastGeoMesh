namespace FastGeoMesh.Domain {
    /// <summary>
    /// Represents an error with a code and description for operation results.
    /// </summary>
    public record Error(string Code, string Description) {
        /// <summary>
        /// Represents the absence of an error.
        /// </summary>
        public static readonly Error None = new(string.Empty, string.Empty);
    }
}
