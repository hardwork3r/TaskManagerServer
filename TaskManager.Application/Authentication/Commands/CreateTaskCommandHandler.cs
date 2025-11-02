namespace TaskManager.Application.Tasks.Commands;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Tasks;
using TaskManager.Contracts.Users;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

public class CreateTaskCommandHandler : ICommandHandler<CreateTaskCommand, TaskWithUsersResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateTaskCommandHandler> _logger;

    public CreateTaskCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateTaskCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TaskWithUsersResponse> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Creating task '{Title}' by user {UserId}",
            request.Title, _currentUserService.UserId);

        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            _logger.LogWarning("Create task failed: User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var assignedUserIds = request.AssignedUserIds ?? new List<string>();
        if (!assignedUserIds.Contains(_currentUserService.UserId))
        {
            assignedUserIds.Add(_currentUserService.UserId);
            _logger.LogDebug("Added creator {UserId} to assigned users", _currentUserService.UserId);
        }

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate,
            Tags = request.Tags ?? new List<string>(),
            UserId = _currentUserService.UserId,
            AssignedUserIds = assignedUserIds
        };

        await _unitOfWork.Tasks.CreateAsync(task, ct);

        _logger.LogInformation("Task created: '{Title}' (ID: {TaskId}, Priority: {Priority}, Status: {Status})",
            task.Title, task.Id, task.Priority, task.Status);

        var assignedUsers = new List<UserDto>();
        foreach (var userId in assignedUserIds)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, ct);
            if (user != null)
            {
                assignedUsers.Add(new UserDto(user.Id, user.Name));
            }
        }

        return new TaskWithUsersResponse(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.Tags,
            task.UserId,
            assignedUsers,
            new List<AttachmentResponse>(),
            task.CreatedAt
        );
    }
}