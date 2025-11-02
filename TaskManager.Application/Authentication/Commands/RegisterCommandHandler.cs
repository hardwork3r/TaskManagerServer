namespace TaskManager.Application.Authentication.Commands;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Authentication;
using TaskManager.Contracts.Users;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, TokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ILogger<RegisterCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<TokenResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Registration attempt for {Email}", request.Email);

        var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email, ct);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: Email already registered {Email}", request.Email);
            throw new InvalidOperationException("Email already registered");
        }

        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            Role = request.Role,
            HashedPassword = _authService.HashPassword(request.Password)
        };

        await _unitOfWork.Users.CreateAsync(user, ct);

        _logger.LogInformation("User registered successfully: {Email} (ID: {UserId}, Role: {Role})",
            user.Email, user.Id, user.Role);

        var accessToken = _authService.CreateAccessToken(user.Id, user.Role);

        return new TokenResponse(
            accessToken,
            "bearer",
            new UserResponse(user.Id, user.Email, user.Name, user.Role, user.CreatedAt)
        );
    }
}
