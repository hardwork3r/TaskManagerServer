namespace TaskManager.Contracts.Common;

public record ApiResponse<T>(T Data, string? Message = null);

public record ApiError(string Detail, int StatusCode);