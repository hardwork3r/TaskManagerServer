namespace TaskManager.Application.Users.Queries;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Users;
using TaskManager.Domain.Interfaces;

public class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, List<UserResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<List<UserResponse>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Getting all users requested by {UserId}", _currentUserService.UserId);

        if (!_currentUserService.IsAdmin)
        {
            _logger.LogWarning("❌ Get all users failed: User {UserId} is not admin",
                _currentUserService.UserId);
            throw new UnauthorizedAccessException("Admin access required");
        }

        var users = await _unitOfWork.Users.GetAllAsync(ct);

        _logger.LogInformation("Retrieved {Count} users", users.Count);

        return users.Select(u => new UserResponse(
            u.Id,
            u.Email,
            u.Name,
            u.Role,
            u.CreatedAt
        )).ToList();
    }
}