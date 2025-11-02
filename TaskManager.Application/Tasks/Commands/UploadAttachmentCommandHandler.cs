namespace TaskManager.Application.Tasks.Commands;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Tasks;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

public class UploadAttachmentCommandHandler : ICommandHandler<UploadAttachmentCommand, AttachmentResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UploadAttachmentCommandHandler> _logger;
    private const long MaxFileSize = 100 * 1024 * 1024; // 100MB

    public UploadAttachmentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UploadAttachmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<AttachmentResponse> Handle(UploadAttachmentCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Uploading attachment '{FileName}' ({FileSize} bytes) to task {TaskId} by user {UserId}",
            request.FileName, request.FileSize, request.TaskId, _currentUserService.UserId);

        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            _logger.LogWarning("Upload attachment failed: User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var task = await _unitOfWork.Tasks.GetByIdAsync(request.TaskId, ct);
        if (task == null)
        {
            _logger.LogWarning("Upload attachment failed: Task {TaskId} not found", request.TaskId);
            throw new KeyNotFoundException("Task not found");
        }

        if (request.FileSize > MaxFileSize)
        {
            _logger.LogWarning("Upload attachment failed: File size {FileSize} exceeds limit of {MaxFileSize}",
                request.FileSize, MaxFileSize);
            throw new InvalidOperationException($"File size exceeds {MaxFileSize / (1024 * 1024)}MB limit");
        }

        _logger.LogDebug("Uploading file to GridFS");
        var gridFsId = await _unitOfWork.Files.UploadAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            ct
        );

        var attachment = new TaskAttachment
        {
            FileName = request.FileName,
            FileSize = request.FileSize,
            ContentType = request.ContentType,
            GridFsId = gridFsId
        };

        task.Attachments.Add(attachment);
        await _unitOfWork.Tasks.UpdateAttachmentsAsync(request.TaskId, task.Attachments, ct);

        _logger.LogInformation("Attachment '{FileName}' (ID: {AttachmentId}) uploaded to task {TaskId}",
            attachment.FileName, attachment.Id, request.TaskId);

        return new AttachmentResponse(
            attachment.Id,
            attachment.FileName,
            attachment.FileSize,
            attachment.ContentType,
            attachment.GridFsId,
            attachment.UploadedAt
        );
    }
}