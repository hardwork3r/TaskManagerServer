namespace TaskManager.Domain.Entities;

public class TaskAttachment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string GridFsId { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}