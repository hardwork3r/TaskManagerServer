namespace TaskManager.Infrastructure.Persistence.Repositories;

using MongoDB.Bson;
using MongoDB.Driver;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

public class TaskRepository : ITaskRepository
{
    private readonly IMongoCollection<TaskItem> _tasks;

    public TaskRepository(IMongoDatabase database)
    {
        _tasks = database.GetCollection<TaskItem>("tasks");
    }

    public async Task<TaskItem?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _tasks.Find(t => t.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<List<TaskItem>> GetAsync(
        string? userId = null,
        string? status = null,
        string? priority = null,
        string? tag = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var filterBuilder = Builders<TaskItem>.Filter;
        var filters = new List<FilterDefinition<TaskItem>>();

        if (!string.IsNullOrEmpty(userId))
        {
            var userFilter = filterBuilder.Or(
                filterBuilder.Eq(t => t.UserId, userId),
                filterBuilder.AnyEq(t => t.AssignedUserIds, userId)
            );
            filters.Add(userFilter);
        }

        if (!string.IsNullOrEmpty(status))
            filters.Add(filterBuilder.Eq(t => t.Status, status));

        if (!string.IsNullOrEmpty(priority))
            filters.Add(filterBuilder.Eq(t => t.Priority, priority));

        if (!string.IsNullOrEmpty(tag))
            filters.Add(filterBuilder.AnyEq(t => t.Tags, tag));

        if (!string.IsNullOrEmpty(search))
        {
            var searchFilter = filterBuilder.Or(
                filterBuilder.Regex(t => t.Title, new BsonRegularExpression(search, "i")),
                filterBuilder.Regex(t => t.Description, new BsonRegularExpression(search, "i"))
            );
            filters.Add(searchFilter);
        }

        var finalFilter = filters.Count > 0
            ? filterBuilder.And(filters)
            : filterBuilder.Empty;

        return await _tasks.Find(finalFilter).ToListAsync(ct);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default)
    {
        await _tasks.InsertOneAsync(task, cancellationToken: ct);
        return task;
    }

    public async Task<bool> UpdateAsync(string id, TaskItem task, CancellationToken ct = default)
    {
        var result = await _tasks.ReplaceOneAsync(t => t.Id == id, task, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var result = await _tasks.DeleteOneAsync(t => t.Id == id, ct);
        return result.DeletedCount > 0;
    }

    public async Task<bool> DeleteByUserIdAsync(string userId, CancellationToken ct = default)
    {
        var result = await _tasks.DeleteManyAsync(t => t.UserId == userId, ct);
        return result.DeletedCount > 0;
    }

    public async Task<bool> UpdateAttachmentsAsync(string id, List<TaskAttachment> attachments, CancellationToken ct = default)
    {
        var update = Builders<TaskItem>.Update.Set(t => t.Attachments, attachments);
        var result = await _tasks.UpdateOneAsync(t => t.Id == id, update, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }
}