namespace TaskManager.Application.Authentication.Queries;

using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Users;

public record GetCurrentUserQuery : IQuery<UserResponse>;