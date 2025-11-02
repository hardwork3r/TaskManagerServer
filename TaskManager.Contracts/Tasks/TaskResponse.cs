namespace TaskManager.Contracts.Tasks;

public record TaskResponse(
    string Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    List<string> Tags,
    string UserId,
    List<string> AssignedUserIds,
    List<AttachmentResponse> Attachments,
    DateTime CreatedAt
);