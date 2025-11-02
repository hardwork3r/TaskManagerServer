namespace TaskManager.Application.Tasks.Commands;

using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Tasks;

public record UploadAttachmentCommand(
    string TaskId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize
) : ICommand<AttachmentResponse>;