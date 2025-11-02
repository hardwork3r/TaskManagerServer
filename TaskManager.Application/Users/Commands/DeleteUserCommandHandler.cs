namespace TaskManager.Application.Users.Commands;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Interfaces;

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Attempting to delete user {UserId} by admin {AdminId}",
            request.UserId, _currentUserService.UserId);

        if (!_currentUserService.IsAdmin)
        {
            _logger.LogWarning("Delete user failed: User {UserId} is not admin",
                _currentUserService.UserId);
            throw new UnauthorizedAccessException("Admin access required");
        }

        if (request.UserId == _currentUserService.UserId)
        {
            _logger.LogWarning("Delete user failed: Admin {UserId} trying to delete themselves",
                _currentUserService.UserId);
            throw new InvalidOperationException("Cannot delete yourself");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user == null)
        {
            _logger.LogWarning("Delete user failed: User {UserId} not found", request.UserId);
            throw new KeyNotFoundException("User not found");
        }

        _logger.LogInformation("Deleting user {Email} (ID: {UserId}) and all their tasks",
            user.Email, user.Id);

        await _unitOfWork.Tasks.DeleteByUserIdAsync(request.UserId, ct);
        var result = await _unitOfWork.Users.DeleteAsync(request.UserId, ct);

        if (result)
        {
            _logger.LogInformation("User {Email} (ID: {UserId}) deleted successfully by admin {AdminId}",
                user.Email, user.Id, _currentUserService.UserId);
        }
        else
        {
            _logger.LogError("Failed to delete user {UserId}", request.UserId);
        }

        return result;
    }
}