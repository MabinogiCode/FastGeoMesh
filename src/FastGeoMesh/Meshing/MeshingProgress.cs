using FastGeoMesh.Structures;

namespace FastGeoMesh.Meshing
{
    /// <summary>
    /// Provides progress information during asynchronous meshing operations.
    /// </summary>
    public readonly struct MeshingProgress
    {
        /// <summary>
        /// Current operation being performed.
        /// </summary>
        public string Operation { get; }

        /// <summary>
        /// Progress percentage (0.0 to 1.0).
        /// </summary>
        public double Percentage { get; }

        /// <summary>
        /// Number of elements processed so far.
        /// </summary>
        public int ProcessedElements { get; }

        /// <summary>
        /// Total number of elements to process.
        /// </summary>
        public int TotalElements { get; }

        /// <summary>
        /// Estimated time remaining for the operation.
        /// </summary>
        public TimeSpan? EstimatedTimeRemaining { get; }

        /// <summary>
        /// Additional status information.
        /// </summary>
        public string? StatusMessage { get; }

        /// <summary>
        /// Creates a new meshing progress instance.
        /// </summary>
        /// <param name="operation">Current operation name.</param>
        /// <param name="percentage">Progress percentage (0.0 to 1.0).</param>
        /// <param name="processedElements">Number of elements processed.</param>
        /// <param name="totalElements">Total number of elements.</param>
        /// <param name="estimatedTimeRemaining">Estimated time remaining.</param>
        /// <param name="statusMessage">Additional status information.</param>
        public MeshingProgress(
            string operation, 
            double percentage, 
            int processedElements, 
            int totalElements,
            TimeSpan? estimatedTimeRemaining = null,
            string? statusMessage = null)
        {
            Operation = operation;
            Percentage = Math.Clamp(percentage, 0.0, 1.0);
            ProcessedElements = processedElements;
            TotalElements = totalElements;
            EstimatedTimeRemaining = estimatedTimeRemaining;
            StatusMessage = statusMessage;
        }

        /// <summary>
        /// Creates a progress instance from processed/total counts.
        /// </summary>
        /// <param name="operation">Current operation name.</param>
        /// <param name="processed">Number of elements processed.</param>
        /// <param name="total">Total number of elements.</param>
        /// <param name="statusMessage">Additional status information.</param>
        /// <returns>Progress instance with calculated percentage.</returns>
        public static MeshingProgress FromCounts(string operation, int processed, int total, string? statusMessage = null)
        {
            double percentage = total > 0 ? (double)processed / total : 0.0;
            return new MeshingProgress(operation, percentage, processed, total, statusMessage: statusMessage);
        }

        /// <summary>
        /// Creates a completed progress instance.
        /// </summary>
        /// <param name="operation">Completed operation name.</param>
        /// <param name="totalElements">Total elements processed.</param>
        /// <returns>Progress instance indicating completion.</returns>
        public static MeshingProgress Completed(string operation, int totalElements)
        {
            return new MeshingProgress(operation, 1.0, totalElements, totalElements, TimeSpan.Zero, "Completed");
        }

        /// <summary>
        /// Returns a string representation of the current progress.
        /// </summary>
        public override string ToString()
        {
            var percentage = (Percentage * 100).ToString("F1");
            var baseMessage = $"{Operation}: {percentage}% ({ProcessedElements}/{TotalElements})";
            
            if (EstimatedTimeRemaining.HasValue)
            {
                baseMessage += $" - ETA: {EstimatedTimeRemaining:mm\\:ss}";
            }
            
            if (!string.IsNullOrEmpty(StatusMessage))
            {
                baseMessage += $" - {StatusMessage}";
            }
            
            return baseMessage;
        }
    }
}
