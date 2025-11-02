namespace TaskManager.Application.Users.Queries;

using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Users;

public record GetAllUsersQuery : IQuery<List<UserResponse>>;