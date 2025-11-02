namespace TaskManager.Application.Tasks.Commands;

using TaskManager.Application.Common.Interfaces;

public record DeleteTaskCommand(string TaskId) : ICommand<bool>;