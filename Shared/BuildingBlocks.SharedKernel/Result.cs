namespace BuildingBlocks.SharedKernel;

public class Result
{
    public bool Success { get; }
    public string Message { get; }

    protected Result(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public static Result Ok(string message = "") => new(true, message);

    public static Result Fail(string message) => new(false, message);
}

public class Result<T> : Result
{
    public T? Data { get; }

    protected Result(bool success, string message, T? data) : base(success, message)
    {
        Data = data;
    }

    public static Result<T> Ok(T data, string message = "") => new(true, message, data);

    public static new Result<T> Fail(string message) => new(false, message, default);
}
