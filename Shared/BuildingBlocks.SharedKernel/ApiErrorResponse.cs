namespace BuildingBlocks.SharedKernel;

public class ApiErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
    public string? TraceId { get; set; }
}
