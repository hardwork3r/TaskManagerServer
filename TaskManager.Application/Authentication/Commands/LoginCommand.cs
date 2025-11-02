namespace TaskManager.Application.Authentication.Commands;

using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Authentication;

public record LoginCommand(
    string Email,
    string Password
) : ICommand<TokenResponse>;