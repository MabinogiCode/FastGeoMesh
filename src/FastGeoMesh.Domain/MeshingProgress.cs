using System;
using System.Globalization;

namespace FastGeoMesh.Meshing
{
    public readonly struct MeshingProgress
    {
        public string Operation { get; }
        public double Percentage { get; }
        public int ProcessedElements { get; }
        public int TotalElements { get; }
        public TimeSpan? EstimatedTimeRemaining { get; }
        public string? StatusMessage { get; }
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
        public static MeshingProgress FromCounts(string operation, int processed, int total, string? statusMessage = null)
        {
            double percentage = total > 0 ? (double)processed / total : 0.0;
            return new MeshingProgress(operation, percentage, processed, total, statusMessage: statusMessage);
        }
        public static MeshingProgress Completed(string operation, int totalElements)
        {
            return new MeshingProgress(operation, 1.0, totalElements, totalElements, TimeSpan.Zero, "Completed");
        }
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
