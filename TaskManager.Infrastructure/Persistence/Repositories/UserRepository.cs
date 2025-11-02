namespace TaskManager.Infrastructure.Persistence.Repositories;

using MongoDB.Driver;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<User>("users");
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync(ct);
    }

    public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await _users.Find(_ => true).ToListAsync(ct);
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        await _users.InsertOneAsync(user, cancellationToken: ct);
        return user;
    }

    public async Task<bool> UpdateAsync(string id, User user, CancellationToken ct = default)
    {
        var result = await _users.ReplaceOneAsync(u => u.Id == id, user, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var result = await _users.DeleteOneAsync(u => u.Id == id, ct);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
    {
        return await _users.Find(u => u.Id == id).AnyAsync(ct);
    }
}