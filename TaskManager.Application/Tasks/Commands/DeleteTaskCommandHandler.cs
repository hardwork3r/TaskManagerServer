namespace TaskManager.Application.Tasks.Commands;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Interfaces;

public class DeleteTaskCommandHandler : ICommandHandler<DeleteTaskCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteTaskCommandHandler> _logger;

    public DeleteTaskCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeleteTaskCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Deleting task {TaskId} by user {UserId}",
            request.TaskId, _currentUserService.UserId);

        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            _logger.LogWarning("Delete task failed: User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var task = await _unitOfWork.Tasks.GetByIdAsync(request.TaskId, ct);
        if (task == null)
        {
            _logger.LogWarning("Delete task failed: Task {TaskId} not found", request.TaskId);
            throw new KeyNotFoundException("Task not found");
        }

        if (!_currentUserService.IsAdmin && task.UserId != _currentUserService.UserId)
        {
            _logger.LogWarning("Delete task failed: User {UserId} is not owner or admin for task {TaskId}",
                _currentUserService.UserId, request.TaskId);
            throw new UnauthorizedAccessException("Access denied");
        }

        _logger.LogInformation("Deleting {AttachmentCount} attachments for task {TaskId}",
            task.Attachments.Count, request.TaskId);

        foreach (var attachment in task.Attachments)
        {
            try
            {
                await _unitOfWork.Files.DeleteAsync(attachment.GridFsId, ct);
                _logger.LogDebug("Deleted attachment {FileName} (ID: {AttachmentId})",
                    attachment.FileName, attachment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete attachment {AttachmentId}", attachment.Id);
            }
        }

        var result = await _unitOfWork.Tasks.DeleteAsync(request.TaskId, ct);

        if (result)
        {
            _logger.LogInformation("Task '{Title}' (ID: {TaskId}) deleted successfully",
                task.Title, request.TaskId);
        }
        else
        {
            _logger.LogError("Failed to delete task {TaskId}", request.TaskId);
        }

        return result;
    }
}
