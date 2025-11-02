using TaskManager.Domain.Common;

namespace TaskManager.Domain.Entities;

public class User : Entity
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public string HashedPassword { get; set; } = string.Empty;
}