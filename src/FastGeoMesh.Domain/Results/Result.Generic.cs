namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Represents the result of an operation that returns a value on success.
    /// </summary>
    /// <typeparam name="TValue">The type of the value returned on success.</typeparam>
    public class Result<TValue> : Result
    {
        private readonly TValue? _value;

        /// <summary>
        /// Gets the value of the successful result.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if accessing value on a failed result.</exception>
        public TValue Value
        {
            get
            {
                if (IsFailure)
                {
                    throw new InvalidOperationException("Cannot access value of a failed result.");
                }
                return _value!;
            }
        }

        /// <summary>
        /// Initializes a new successful result with the specified value.
        /// </summary>
        /// <param name="value">The value of the successful result.</param>
        private Result(TValue value) : base(true, Error.None)
        {
            _value = value;
        }

        /// <summary>
        /// Initializes a new failed result with the specified error.
        /// </summary>
        /// <param name="error">The error associated with the failure.</param>
        private Result(Error error) : base(false, error)
        {
            _value = default;
        }

        /// <summary>
        /// Creates a successful result with the specified value.
        /// </summary>
        /// <param name="value">The value of the successful result.</param>
        public static Result<TValue> Success(TValue value) => new(value);

        /// <summary>
        /// Creates a failed result with the specified error.
        /// </summary>
        /// <param name="error">The error associated with the failure.</param>
        public new static Result<TValue> Failure(Error error) => new(error);

        /// <summary>
        /// Provides a way to handle both success and failure cases.
        /// </summary>
        /// <param name="onSuccess">Function to call on success.</param>
        /// <param name="onFailure">Function to call on failure.</param>
        /// <returns>The return value of the executed function.</returns>
        public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<Error, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value) : onFailure(Error);
        }

        /// <summary>
        /// Implicitly converts a value to a successful result.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static implicit operator Result<TValue>(TValue value) => Success(value);

        /// <summary>
        /// Implicitly converts an error to a failed result.
        /// </summary>
        /// <param name="error">The error to convert.</param>
        public static implicit operator Result<TValue>(Error error) => Failure(error);
    }
}
