namespace TaskManager.Contracts.Authentication;

using System.ComponentModel.DataAnnotations;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string Name,
    string Role = "user"
);