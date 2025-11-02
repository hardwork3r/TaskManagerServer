namespace TaskManager.Infrastructure.Persistence;

using MongoDB.Driver;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    public IUserRepository Users { get; }
    public ITaskRepository Tasks { get; }
    public IFileRepository Files { get; }

    public UnitOfWork(IMongoDatabase database)
    {
        Users = new UserRepository(database);
        Tasks = new TaskRepository(database);
        Files = new FileRepository(database);
    }
}