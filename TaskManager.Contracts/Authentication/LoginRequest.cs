namespace TaskManager.Contracts.Authentication;

using System.ComponentModel.DataAnnotations;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);