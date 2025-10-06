namespace FastGeoMesh.Utils
{
    /// <summary>
    /// Performance-optimized extensions for ValueTask to improve async operations.
    /// </summary>
    internal static class ValueTaskExtensions
    {
        /// <summary>
        /// Creates a continuation for ValueTask that transforms the result.
        /// </summary>
        /// <typeparam name="TSource">Source result type.</typeparam>
        /// <typeparam name="TResult">Target result type.</typeparam>
        /// <param name="valueTask">Source ValueTask.</param>
        /// <param name="continuationFunction">Function to transform the result.</param>
        /// <param name="continuationOptions">Continuation options.</param>
        /// <returns>ValueTask with transformed result.</returns>
        public static ValueTask<TResult> ContinueWith<TSource, TResult>(
            this ValueTask<TSource> valueTask,
            Func<TSource, TResult> continuationFunction,
            TaskContinuationOptions continuationOptions = TaskContinuationOptions.None)
        {
            ArgumentNullException.ThrowIfNull(continuationFunction);

            if (valueTask.IsCompletedSuccessfully)
            {
                // Fast path: if already completed successfully, apply transformation immediately
                try
                {
                    return new ValueTask<TResult>(continuationFunction(valueTask.Result));
                }
                catch
                {
                    // If transformation fails, let it propagate
                    throw;
                }
            }

            // Slow path: create a proper Task continuation for incomplete or faulted tasks
            return new ValueTask<TResult>(valueTask.AsTask().ContinueWith(
                task => 
                {
                    // The task.Result access will throw if the task faulted or was canceled
                    // This ensures proper exception propagation
                    return continuationFunction(task.Result);
                },
                continuationOptions));
        }
    }
}
