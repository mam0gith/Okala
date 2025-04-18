namespace Okala.Shared
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public T? Data { get; set; }

        public static Result<T> Success(T data, string? message = null)
            => new Result<T> { IsSuccess = true, Data = data, Message = message };

        public static Result<T> Failure(string message, List<string>? errors = null)
            => new Result<T> { IsSuccess = false, Message = message, Errors = errors };
    }

    public static class Result
    {
        public static Result<T> Success<T>(T data, string? message = null) => Result<T>.Success(data, message);
        public static Result<T> Failure<T>(string message, List<string>? errors = null) => Result<T>.Failure(message, errors);
    }

}
