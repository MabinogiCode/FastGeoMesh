using System.Globalization;
using FastGeoMesh.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastGeoMesh.Tests.Coverage
{
    /// <summary>
    /// Tests for utility classes and helper methods to improve code coverage.
    /// Covers ValueTaskExtensions and other internal utilities.
    /// </summary>
    public sealed class UtilityCoverageTests
    {
        /// <summary>Tests ValueTaskExtensions.ContinueWith with completed ValueTask.</summary>
        [Fact]
        public async Task ValueTaskExtensionsContinueWithCompletedValueTaskUsesFastPath()
        {
            // Arrange
            var completedValueTask = new ValueTask<int>(42);

            // Act
            var result = await completedValueTask.ContinueWith(
                value => value * 2,
                TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(true);

            // Assert
            result.Should().Be(84);
        }

        /// <summary>Tests ValueTaskExtensions.ContinueWith with non-completed ValueTask.</summary>
        [Fact]
        public async Task ValueTaskExtensionsContinueWithNonCompletedValueTaskUsesAsyncPath()
        {
            // Arrange
            var tcs = new TaskCompletionSource<int>();
            var valueTask = new ValueTask<int>(tcs.Task);

            // Start continuation
            var continuationTask = valueTask.ContinueWith(
                value => value * 3,
                TaskContinuationOptions.ExecuteSynchronously);

            // Complete the original task
            tcs.SetResult(10);

            // Act
            var result = await continuationTask.ConfigureAwait(true);

            // Assert
            result.Should().Be(30);
        }

        /// <summary>Tests ValueTaskExtensions.ContinueWith with different continuation options.</summary>
        [Theory]
        [InlineData(TaskContinuationOptions.None)]
        [InlineData(TaskContinuationOptions.ExecuteSynchronously)]
        [InlineData(TaskContinuationOptions.RunContinuationsAsynchronously)]
        public async Task ValueTaskExtensionsContinueWithWithDifferentOptionsWorksCorrectly(TaskContinuationOptions options)
        {
            // Arrange
            var completedValueTask = new ValueTask<string>("Hello");

            // Act
            var result = await completedValueTask.ContinueWith(
                value => value + " World",
                options).ConfigureAwait(true);

            // Assert
            result.Should().Be("Hello World");
        }

        /// <summary>Tests ValueTaskExtensions.ContinueWith with transformation function.</summary>
        [Fact]
        public async Task ValueTaskExtensionsContinueWithTransformsDifferentTypes()
        {
            // Arrange
            var valueTask = new ValueTask<int>(123);

            // Act
            var stringResult = await valueTask.ContinueWith(
                value => $"Number: {value}",
                TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(true);

            var boolResult = await valueTask.ContinueWith(
                value => value > 100,
                TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(true);

            // Assert
            stringResult.Should().Be("Number: 123");
            boolResult.Should().BeTrue();
        }

        /// <summary>Tests ValueTaskExtensions with exception handling.</summary>
        [Fact]
        public async Task ValueTaskExtensionsContinueWithWithExceptionPropagatesException()
        {
            // Arrange
            var tcs = new TaskCompletionSource<int>();
            var valueTask = new ValueTask<int>(tcs.Task);

            var continuationTask = valueTask.ContinueWith(
                value => value * 2,
                TaskContinuationOptions.ExecuteSynchronously);

            // Set exception
            tcs.SetException(new InvalidOperationException("Test exception"));

            // Act & Assert - Expect AggregateException containing the original exception
            var exception = await Assert.ThrowsAsync<AggregateException>(() => continuationTask.AsTask()).ConfigureAwait(true);
            exception.InnerException.Should().BeOfType<InvalidOperationException>();
            exception.InnerException.Message.Should().Be("Test exception");
        }

        /// <summary>Tests ValueTaskExtensions with cancellation.</summary>
        [Fact]
        public async Task ValueTaskExtensionsContinueWithWithCancellationPropagatesCancellation()
        {
            // Arrange
            var tcs = new TaskCompletionSource<int>();
            var valueTask = new ValueTask<int>(tcs.Task);

            var continuationTask = valueTask.ContinueWith(
                value => value * 2,
                TaskContinuationOptions.ExecuteSynchronously);

            // Cancel the original task
            tcs.SetCanceled();

            // Act & Assert - Expect AggregateException containing TaskCanceledException
            var exception = await Assert.ThrowsAsync<AggregateException>(() => continuationTask.AsTask()).ConfigureAwait(true);
            exception.InnerException.Should().BeOfType<TaskCanceledException>();
        }

        /// <summary>Tests complex transformation chain with ValueTaskExtensions.</summary>
        [Fact]
        public async Task ValueTaskExtensionsContinueWithChainedTransformationsWorksCorrectly()
        {
            // Arrange
            var initialValueTask = new ValueTask<int>(5);

            // Act - Chain multiple transformations
            var result = await initialValueTask
                .ContinueWith(x => x * 2, TaskContinuationOptions.ExecuteSynchronously)
                .ContinueWith(x => x + 10, TaskContinuationOptions.ExecuteSynchronously)
                .ContinueWith(x => x.ToString(CultureInfo.InvariantCulture), TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(true);

            // Assert
            result.Should().Be("20"); // (5 * 2) + 10 = 20
        }

        /// <summary>Tests ValueTaskExtensions with null transformation function.</summary>
        [Fact]
        public async Task ValueTaskExtensionsContinueWithWithNullFunctionThrowsException()
        {
            // Arrange
            var valueTask = new ValueTask<int>(42);
            Func<int, string> nullFunction = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                valueTask.ContinueWith(nullFunction).AsTask()).ConfigureAwait(true);
        }

        /// <summary>Tests edge case with very large numbers.</summary>
        [Fact]
        public async Task ValueTaskExtensionsContinueWithWithLargeNumbersHandlesCorrectly()
        {
            // Arrange
            var valueTask = new ValueTask<long>(long.MaxValue);

            // Act
            var result = await valueTask.ContinueWith(
                value => value / 2,
                TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(true);

            // Assert
            result.Should().Be(long.MaxValue / 2);
        }

        /// <summary>Tests ValueTaskExtensions with generic types.</summary>
        [Fact]
        public async Task ValueTaskExtensionsContinueWithWithGenericTypesWorksCorrectly()
        {
            // Arrange
            var valueTask = new ValueTask<List<int>>(new List<int> { 1, 2, 3 });

            // Act
            var result = await valueTask.ContinueWith(
                list => list.Count,
                TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(true);

            // Assert
            result.Should().Be(3);
        }

        /// <summary>Tests ValueTaskExtensions with struct types.</summary>
        [Fact]
        public async Task ValueTaskExtensionsContinueWithWithStructTypesWorksCorrectly()
        {
            // Arrange
            var valueTask = new ValueTask<DateTime>(new DateTime(2024, 1, 1));

            // Act
            var result = await valueTask.ContinueWith(
                date => date.Year,
                TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(true);

            // Assert
            result.Should().Be(2024);
        }

        /// <summary>Tests ValueTaskExtensions performance with many operations.</summary>
        [Fact]
        public async Task ValueTaskExtensionsContinueWithPerformanceTestCompletesQuickly()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var tasks = new List<ValueTask<int>>();

            // Act - Create many continuation operations
            for (int i = 0; i < 100; i++)
            {
                var valueTask = new ValueTask<int>(i);
                var continuationTask = valueTask.ContinueWith(
                    x => x * 2,
                    TaskContinuationOptions.ExecuteSynchronously);
                tasks.Add(continuationTask);
            }

            var results = new List<int>();
            foreach (var task in tasks)
            {
                results.Add(await task.ConfigureAwait(true));
            }

            stopwatch.Stop();

            // Assert
            results.Should().HaveCount(100);
            results[0].Should().Be(0);  // 0 * 2
            results[99].Should().Be(198); // 99 * 2
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should be very fast
        }
    }
}
