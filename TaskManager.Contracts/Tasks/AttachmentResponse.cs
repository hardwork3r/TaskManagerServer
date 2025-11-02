namespace TaskManager.Contracts.Tasks;

public record AttachmentResponse(
    string Id,
    string FileName,
    long FileSize,
    string ContentType,
    string GridFsId,
    DateTime UploadedAt
);