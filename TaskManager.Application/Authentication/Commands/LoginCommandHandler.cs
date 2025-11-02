namespace TaskManager.Application.Authentication.Commands;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Authentication;
using TaskManager.Contracts.Users;
using TaskManager.Domain.Interfaces;

public class LoginCommandHandler : ICommandHandler<LoginCommand, TokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Login attempt for {Email}", request.Email);

        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, ct);

        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        _logger.LogDebug("User found: {UserId}, verifying password", user.Id);

        if (!_authService.VerifyPassword(request.Password, user.HashedPassword))
        {
            _logger.LogWarning("Login failed: Invalid password for {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var accessToken = _authService.CreateAccessToken(user.Id, user.Role);

        _logger.LogInformation("User {Email} (ID: {UserId}, Role: {Role}) logged in successfully",
            user.Email, user.Id, user.Role);

        return new TokenResponse(
            accessToken,
            "bearer",
            new UserResponse(user.Id, user.Email, user.Name, user.Role, user.CreatedAt)
        );
    }
}