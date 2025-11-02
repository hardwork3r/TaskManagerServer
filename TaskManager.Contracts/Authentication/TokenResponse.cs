using TaskManager.Contracts.Users;

namespace TaskManager.Contracts.Authentication;

public record TokenResponse(
    string AccessToken,
    string TokenType,
    UserResponse User
);
