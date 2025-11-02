namespace TaskManager.Application.Tasks.Commands;

using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Tasks;

public record UpdateTaskCommand(
    string TaskId,
    string? Title,
    string? Description,
    string? Status,
    string? Priority,
    DateTime? DueDate,
    List<string>? Tags,
    List<string>? AssignedUserIds
) : ICommand<TaskWithUsersResponse>;