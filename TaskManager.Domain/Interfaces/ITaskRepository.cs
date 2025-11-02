namespace TaskManager.Domain.Interfaces;

using TaskManager.Domain.Entities;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<List<TaskItem>> GetAsync(
        string? userId = null,
        string? status = null,
        string? priority = null,
        string? tag = null,
        string? search = null,
        CancellationToken ct = default);
    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default);
    Task<bool> UpdateAsync(string id, TaskItem task, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    Task<bool> DeleteByUserIdAsync(string userId, CancellationToken ct = default);
    Task<bool> UpdateAttachmentsAsync(string id, List<TaskAttachment> attachments, CancellationToken ct = default);
}