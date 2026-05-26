public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool HasError { get; set; }
    public int? ErrorCode { get; set; }

    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T>
        {
            Data = data,
            HasError = false,
            ErrorCode = null
        };
    }

    public static ApiResponse<T> Failure(int errorCode)
    {
        return new ApiResponse<T>
        {
            Data = default,
            HasError = true,
            ErrorCode = errorCode
        };
    }
}