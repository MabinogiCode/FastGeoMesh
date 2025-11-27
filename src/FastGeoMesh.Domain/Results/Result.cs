namespace FastGeoMesh.Domain
{
    /// <summary>
    /// Represents the result of an operation, indicating success or failure and containing an error if failed.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }
        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;
        /// <summary>
        /// Gets the error associated with the result, or <see cref="Error.None"/> if successful.
        /// </summary>
        public Error Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class.
        /// </summary>
        /// <param name="isSuccess">Indicates if the result is a success.</param>
        /// <param name="error">The error associated with the result.</param>
        /// <exception cref="InvalidOperationException">Thrown if the error state is invalid.</exception>
        protected Result(bool isSuccess, Error error)
        {
            if ((isSuccess && error != Error.None) || (!isSuccess && error == Error.None))
            {
                throw new InvalidOperationException("Invalid error state for result.");
            }
            IsSuccess = isSuccess;
            Error = error;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static Result Success() => new(true, Error.None);
        /// <summary>
        /// Creates a failed result with the specified error.
        /// </summary>
        /// <param name="error">The error associated with the failure.</param>
        public static Result Failure(Error error) => new(false, error);

        /// <summary>
        /// Implicitly converts an <see cref="Error"/> to a failed <see cref="Result"/>.
        /// </summary>
        /// <param name="error">The error to convert.</param>
        public static implicit operator Result(Error error) => Failure(error);

        /// <summary>
        /// Returns a string representation of the result.
        /// </summary>
        public override string ToString()
        {
            return IsSuccess ? "Success" : $"Failure: {Error}";
        }
    }
}
