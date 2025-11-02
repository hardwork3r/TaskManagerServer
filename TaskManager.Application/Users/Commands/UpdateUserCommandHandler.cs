namespace TaskManager.Application.Users.Commands;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Users;
using TaskManager.Domain.Interfaces;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UserResponse> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Updating user {UserId} by admin {AdminId}",
            request.UserId, _currentUserService.UserId);

        if (!_currentUserService.IsAdmin)
        {
            _logger.LogWarning("Update user failed: User {UserId} is not admin",
                _currentUserService.UserId);
            throw new UnauthorizedAccessException("Admin access required");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user == null)
        {
            _logger.LogWarning("Update user failed: User {UserId} not found", request.UserId);
            throw new KeyNotFoundException("User not found");
        }

        _logger.LogDebug("Updating user {Email} (ID: {UserId})", user.Email, user.Id);

        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email, ct);
            if (existingUser != null && existingUser.Id != request.UserId)
            {
                _logger.LogWarning("Update user failed: Email {Email} already taken", request.Email);
                throw new InvalidOperationException("Email already registered");
            }
        }

        var oldEmail = user.Email;
        var oldRole = user.Role;

        if (!string.IsNullOrEmpty(request.Name))
            user.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Email))
            user.Email = request.Email;

        if (!string.IsNullOrEmpty(request.Role))
            user.Role = request.Role;

        await _unitOfWork.Users.UpdateAsync(request.UserId, user, ct);

        _logger.LogInformation("User {UserId} updated: Email {OldEmail} → {NewEmail}, Role {OldRole} → {NewRole}",
            user.Id, oldEmail, user.Email, oldRole, user.Role);

        return new UserResponse(
            user.Id,
            user.Email,
            user.Name,
            user.Role,
            user.CreatedAt
        );
    }
}