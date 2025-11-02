namespace TaskManager.Application.Users.Commands;

using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Users;

public record UpdateUserCommand(
    string UserId,
    string? Name,
    string? Email,
    string? Role
) : ICommand<UserResponse>;