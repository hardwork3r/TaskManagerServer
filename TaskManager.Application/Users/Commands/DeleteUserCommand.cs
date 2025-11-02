namespace TaskManager.Application.Users.Commands;

using TaskManager.Application.Common.Interfaces;

public record DeleteUserCommand(string UserId) : ICommand<bool>;