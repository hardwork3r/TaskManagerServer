namespace TaskManager.Contracts.Users;

public record UserResponse(
    string Id,
    string Email,
    string Name,
    string Role,
    DateTime CreatedAt
);