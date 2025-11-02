namespace TaskManager.Contracts.Tasks;

using System.ComponentModel.DataAnnotations;

public record CreateTaskRequest(
    [Required] string Title,
    string Description = "",
    string Status = "todo",
    string Priority = "medium",
    DateTime? DueDate = null,
    List<string>? Tags = null,
    List<string>? AssignedUserIds = null
);
