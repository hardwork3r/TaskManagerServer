namespace TaskManager.Application.Tasks.Queries;

using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Tasks;

public record GetTasksQuery(
    string? Status = null,
    string? Priority = null,
    string? Tag = null,
    string? Search = null
) : IQuery<List<TaskWithUsersResponse>>;