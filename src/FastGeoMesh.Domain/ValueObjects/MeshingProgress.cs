using System.Globalization;

namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Represents progress information for meshing operations.
    /// </summary>
    public readonly struct MeshingProgress
    {
        /// <summary>
        /// Gets the name of the current operation being performed.
        /// </summary>
        public string Operation { get; }

        /// <summary>
        /// Gets the completion percentage as a value between 0.0 and 1.0.
        /// </summary>
        public double Percentage { get; }

        /// <summary>
        /// Gets the number of elements that have been processed.
        /// </summary>
        public int ProcessedElements { get; }

        /// <summary>
        /// Gets the total number of elements to be processed.
        /// </summary>
        public int TotalElements { get; }

        /// <summary>
        /// Gets the estimated time remaining for the operation, if available.
        /// </summary>
        public TimeSpan? EstimatedTimeRemaining { get; }

        /// <summary>
        /// Gets an optional status message providing additional context.
        /// </summary>
        public string? StatusMessage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshingProgress"/> struct.
        /// </summary>
        /// <param name="operation">The name of the operation being performed.</param>
        /// <param name="percentage">The completion percentage (0.0 to 1.0).</param>
        /// <param name="processedElements">The number of elements processed.</param>
        /// <param name="totalElements">The total number of elements to process.</param>
        /// <param name="estimatedTimeRemaining">Optional estimated time remaining.</param>
        /// <param name="statusMessage">Optional status message.</param>
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
        /// Creates a new <see cref="MeshingProgress"/> from element counts.
        /// </summary>
        /// <param name="operation">The name of the operation being performed.</param>
        /// <param name="processed">The number of elements processed.</param>
        /// <param name="total">The total number of elements to process.</param>
        /// <param name="statusMessage">Optional status message.</param>
        /// <returns>A new instance of <see cref="MeshingProgress"/>.</returns>
        public static MeshingProgress FromCounts(string operation, int processed, int total, string? statusMessage = null)
        {
            double percentage = total > 0 ? (double)processed / total : 0.0;
            return new MeshingProgress(operation, percentage, processed, total, statusMessage: statusMessage);
        }

        /// <summary>
        /// Creates a new <see cref="MeshingProgress"/> representing a completed operation.
        /// </summary>
        /// <param name="operation">The name of the operation that completed.</param>
        /// <param name="totalElements">The total number of elements that were processed.</param>
        /// <returns>A new instance of <see cref="MeshingProgress"/> representing completion.</returns>
        public static MeshingProgress Completed(string operation, int totalElements)
        {
            return new MeshingProgress(operation, 1.0, totalElements, totalElements, TimeSpan.Zero, "Completed");
        }

        /// <summary>
        /// Returns a string representation of the meshing progress.
        /// </summary>
        /// <returns>A formatted string showing operation name, percentage, counts, and optional additional information.</returns>
        public override string ToString()
        {
            var percentage = (Percentage * 100).ToString("F1", CultureInfo.InvariantCulture);
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
