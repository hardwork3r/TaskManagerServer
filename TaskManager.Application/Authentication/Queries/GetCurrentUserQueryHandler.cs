namespace TaskManager.Application.Authentication.Queries;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Users;
using TaskManager.Domain.Interfaces;

public class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, UserResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetCurrentUserQueryHandler> _logger;

    public GetCurrentUserQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetCurrentUserQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UserResponse> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        _logger.LogDebug("Getting current user info");

        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            _logger.LogWarning("Get current user failed: User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(_currentUserService.UserId, ct);
        if (user == null)
        {
            _logger.LogError("User {UserId} not found in database", _currentUserService.UserId);
            throw new UnauthorizedAccessException("User not found");
        }

        _logger.LogDebug("Current user info retrieved: {Email} (ID: {UserId})", user.Email, user.Id);

        return new UserResponse(user.Id, user.Email, user.Name, user.Role, user.CreatedAt);
    }
}
