namespace TaskManager.Application.Tasks.Queries;

using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Tasks;

public record GetTaskByIdQuery(string TaskId) : IQuery<TaskResponse>;