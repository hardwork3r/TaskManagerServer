namespace TaskManager.Application.Tasks.Queries;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Interfaces;

public class DownloadAttachmentQueryHandler
    : IQueryHandler<DownloadAttachmentQuery, (Stream Stream, string FileName, string ContentType)>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DownloadAttachmentQueryHandler> _logger;

    public DownloadAttachmentQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DownloadAttachmentQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<(Stream Stream, string FileName, string ContentType)> Handle(
        DownloadAttachmentQuery request,
        CancellationToken ct)
    {
        _logger.LogInformation("Downloading attachment {AttachmentId} from task {TaskId} by user {UserId}",
            request.AttachmentId, request.TaskId, _currentUserService.UserId);

        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            _logger.LogWarning("Download attachment failed: User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var task = await _unitOfWork.Tasks.GetByIdAsync(request.TaskId, ct);
        if (task == null)
        {
            _logger.LogWarning("Download attachment failed: Task {TaskId} not found", request.TaskId);
            throw new KeyNotFoundException("Task not found");
        }

        var isAuthorized = _currentUserService.IsAdmin
                           || task.UserId == _currentUserService.UserId
                           || task.AssignedUserIds.Contains(_currentUserService.UserId);

        if (!isAuthorized)
        {
            _logger.LogWarning("Download attachment failed: User {UserId} has no access to task {TaskId}",
                _currentUserService.UserId, request.TaskId);
            throw new UnauthorizedAccessException("Access denied");
        }

        var attachment = task.Attachments.FirstOrDefault(a => a.Id == request.AttachmentId);
        if (attachment == null)
        {
            _logger.LogWarning("Download attachment failed: Attachment {AttachmentId} not found in task {TaskId}",
                request.AttachmentId, request.TaskId);
            throw new KeyNotFoundException("Attachment not found");
        }

        _logger.LogDebug("Downloading file '{FileName}' from GridFS", attachment.FileName);

        var result = await _unitOfWork.Files.DownloadAsync(attachment.GridFsId, ct);

        _logger.LogInformation("Attachment '{FileName}' (ID: {AttachmentId}, Size: {FileSize} bytes) downloaded",
            attachment.FileName, request.AttachmentId, attachment.FileSize);

        return result;
    }
}