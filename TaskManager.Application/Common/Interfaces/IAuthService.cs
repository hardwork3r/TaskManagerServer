namespace TaskManager.Application.Common.Interfaces;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    string CreateAccessToken(string userId, string role);
    string? GetUserIdFromToken(string token);
}