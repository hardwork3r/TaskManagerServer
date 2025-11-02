namespace TaskManager.Application.Authentication.Commands;

using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Authentication;

public record RegisterCommand(
    string Email,
    string Password,
    string Name,
    string Role = "user"
) : ICommand<TokenResponse>;