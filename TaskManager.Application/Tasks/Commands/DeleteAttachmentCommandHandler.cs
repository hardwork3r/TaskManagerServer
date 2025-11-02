namespace TaskManager.Application.Tasks.Commands;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Interfaces;

public class DeleteAttachmentCommandHandler : ICommandHandler<DeleteAttachmentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteAttachmentCommandHandler> _logger;

    public DeleteAttachmentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeleteAttachmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteAttachmentCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Deleting attachment {AttachmentId} from task {TaskId} by user {UserId}",
            request.AttachmentId, request.TaskId, _currentUserService.UserId);

        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            _logger.LogWarning("Delete attachment failed: User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var task = await _unitOfWork.Tasks.GetByIdAsync(request.TaskId, ct);
        if (task == null)
        {
            _logger.LogWarning("Delete attachment failed: Task {TaskId} not found", request.TaskId);
            throw new KeyNotFoundException("Task not found");
        }

        if (!_currentUserService.IsAdmin && task.UserId != _currentUserService.UserId)
        {
            _logger.LogWarning("Delete attachment failed: User {UserId} is not owner or admin for task {TaskId}",
                _currentUserService.UserId, request.TaskId);
            throw new UnauthorizedAccessException("Access denied");
        }

        var attachment = task.Attachments.FirstOrDefault(a => a.Id == request.AttachmentId);
        if (attachment == null)
        {
            _logger.LogWarning("Delete attachment failed: Attachment {AttachmentId} not found in task {TaskId}",
                request.AttachmentId, request.TaskId);
            throw new KeyNotFoundException("Attachment not found");
        }

        _logger.LogDebug("Deleting file '{FileName}' from GridFS", attachment.FileName);

        try
        {
            await _unitOfWork.Files.DeleteAsync(attachment.GridFsId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file from GridFS: {GridFsId}", attachment.GridFsId);
        }

        task.Attachments.Remove(attachment);
        await _unitOfWork.Tasks.UpdateAttachmentsAsync(request.TaskId, task.Attachments, ct);

        _logger.LogInformation("Attachment '{FileName}' (ID: {AttachmentId}) deleted from task {TaskId}",
            attachment.FileName, request.AttachmentId, request.TaskId);

        return true;
    }
}
