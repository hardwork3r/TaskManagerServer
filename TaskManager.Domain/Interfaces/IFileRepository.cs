namespace TaskManager.Domain.Interfaces;

public interface IFileRepository
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task<(Stream Stream, string FileName, string ContentType)> DownloadAsync(string gridFsId, CancellationToken ct = default);
    Task DeleteAsync(string gridFsId, CancellationToken ct = default);
}
