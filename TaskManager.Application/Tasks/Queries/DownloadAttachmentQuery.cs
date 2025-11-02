namespace TaskManager.Application.Tasks.Queries;

using TaskManager.Application.Common.Interfaces;

public record DownloadAttachmentQuery(string TaskId, string AttachmentId)
    : IQuery<(Stream Stream, string FileName, string ContentType)>;