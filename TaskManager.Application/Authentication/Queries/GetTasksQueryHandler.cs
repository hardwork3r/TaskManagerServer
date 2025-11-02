namespace TaskManager.Application.Tasks.Queries;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Tasks;
using TaskManager.Contracts.Users;
using TaskManager.Domain.Interfaces;

public class GetTasksQueryHandler : IQueryHandler<GetTasksQuery, List<TaskWithUsersResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetTasksQueryHandler> _logger;

    public GetTasksQueryHandler(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        ILogger<GetTasksQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<List<TaskWithUsersResponse>> Handle(GetTasksQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Getting tasks for user {UserId} with filters - Status: {Status}, Priority: {Priority}, Tag: {Tag}, Search: {Search}",
            _currentUserService.UserId, request.Status, request.Priority, request.Tag, request.Search);

        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            _logger.LogWarning("Get tasks failed: User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var userId = _currentUserService.IsAdmin ? null : _currentUserService.UserId;

        var tasks = await _unitOfWork.Tasks.GetAsync(
            userId,
            request.Status,
            request.Priority,
            request.Tag,
            request.Search,
            ct
        );

        _logger.LogInformation("Retrieved {Count} tasks", tasks.Count);

        var result = new List<TaskWithUsersResponse>();
        foreach (var task in tasks)
        {
            var assignedUsers = new List<UserDto>();
            foreach (var assignedUserId in task.AssignedUserIds)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(assignedUserId, ct);
                if (user != null)
                {
                    assignedUsers.Add(new UserDto(user.Id, user.Name));
                }
            }

            var attachments = task.Attachments.Select(a => new AttachmentResponse(
                a.Id, a.FileName, a.FileSize, a.ContentType, a.GridFsId, a.UploadedAt
            )).ToList();

            result.Add(new TaskWithUsersResponse(
                task.Id, task.Title, task.Description, task.Status, task.Priority,
                task.DueDate, task.Tags, task.UserId, assignedUsers, attachments, task.CreatedAt
            ));
        }

        return result;
    }
}
