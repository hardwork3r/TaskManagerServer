namespace TaskManager.Application.Tasks.Queries;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Tasks;
using TaskManager.Domain.Interfaces;

public class GetTaskByIdQueryHandler : IQueryHandler<GetTaskByIdQuery, TaskResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetTaskByIdQueryHandler> _logger;

    public GetTaskByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<GetTaskByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TaskResponse> Handle(GetTaskByIdQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Getting task {TaskId} by user {UserId}",
            request.TaskId, _currentUserService.UserId);

        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            _logger.LogWarning("Get task failed: User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var task = await _unitOfWork.Tasks.GetByIdAsync(request.TaskId, ct);
        if (task == null)
        {
            _logger.LogWarning("Get task failed: Task {TaskId} not found", request.TaskId);
            throw new KeyNotFoundException("Task not found");
        }

        if (!_currentUserService.IsAdmin && task.UserId != _currentUserService.UserId)
        {
            _logger.LogWarning("Get task failed: User {UserId} has no access to task {TaskId}",
                _currentUserService.UserId, request.TaskId);
            throw new UnauthorizedAccessException("Access denied");
        }

        _logger.LogDebug("Task {TaskId} retrieved: '{Title}'", task.Id, task.Title);

        var attachments = task.Attachments.Select(a => new AttachmentResponse(
            a.Id, a.FileName, a.FileSize, a.ContentType, a.GridFsId, a.UploadedAt
        )).ToList();

        return new TaskResponse(
            task.Id, task.Title, task.Description, task.Status, task.Priority,
            task.DueDate, task.Tags, task.UserId, task.AssignedUserIds, attachments, task.CreatedAt
        );
    }
}