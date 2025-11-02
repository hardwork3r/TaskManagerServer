using TaskManager.Domain.Common;

namespace TaskManager.Domain.Entities;

public class TaskItem : Entity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "todo";
    public string Priority { get; set; } = "medium";
    public DateTime? DueDate { get; set; }
    public List<string> Tags { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
    public List<string> AssignedUserIds { get; set; } = new();
    public List<TaskAttachment> Attachments { get; set; } = new();
}
