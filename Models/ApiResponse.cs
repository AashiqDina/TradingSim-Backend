public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool HasError { get; set; } = false;
    public int? ErrorCode { get; set; }
}