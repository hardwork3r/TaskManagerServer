namespace TaskManager.Contracts.Tasks;

using TaskManager.Contracts.Users;

public record TaskWithUsersResponse(
    string Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    List<string> Tags,
    string UserId,
    List<UserDto> AssignedUsers,
    List<AttachmentResponse> Attachments,
    DateTime CreatedAt
);