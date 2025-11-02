namespace TaskManager.Contracts.Tasks;

public record UpdateTaskRequest(
    string? Title = null,
    string? Description = null,
    string? Status = null,
    string? Priority = null,
    DateTime? DueDate = null,
    List<string>? Tags = null,
    List<string>? AssignedUserIds = null
);