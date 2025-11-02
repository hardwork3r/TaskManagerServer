namespace TaskManager.Domain.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ITaskRepository Tasks { get; }
    IFileRepository Files { get; }
}