namespace TaskManager.Contracts.Tasks;

public record TaskQueryParams(
    string? Status = null,
    string? Priority = null,
    string? Tag = null,
    string? Search = null
);