namespace TaskManager.Contracts.Users;

using System.ComponentModel.DataAnnotations;

public record UpdateUserRequest(
    string? Name = null,
    [EmailAddress] string? Email = null,
    string? Role = null
);