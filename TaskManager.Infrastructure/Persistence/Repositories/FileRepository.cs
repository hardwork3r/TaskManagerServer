namespace TaskManager.Infrastructure.Persistence.Repositories;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using TaskManager.Domain.Interfaces;

public class FileRepository : IFileRepository
{
    private readonly IGridFSBucket _gridFsBucket;

    public FileRepository(IMongoDatabase database)
    {
        _gridFsBucket = new GridFSBucket(database);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        var options = new GridFSUploadOptions
        {
            Metadata = new BsonDocument
            {
                { "fileName", fileName },
                { "contentType", contentType },
                { "uploadDate", DateTime.UtcNow }
            }
        };

        var objectId = await _gridFsBucket.UploadFromStreamAsync(fileName, fileStream, options, ct);
        return objectId.ToString();
    }

    public async Task<(Stream Stream, string FileName, string ContentType)> DownloadAsync(string gridFsId, CancellationToken ct = default)
    {
        var objectId = new ObjectId(gridFsId);
        var fileInfo = await _gridFsBucket.FindAsync(Builders<GridFSFileInfo>.Filter.Eq("_id", objectId), cancellationToken: ct);
        var file = await fileInfo.FirstOrDefaultAsync(ct);

        if (file == null)
            throw new FileNotFoundException("File not found");

        var stream = await _gridFsBucket.OpenDownloadStreamAsync(objectId, cancellationToken: ct);
        var fileName = file.Metadata?["fileName"]?.AsString ?? file.Filename;
        var contentType = file.Metadata?["contentType"]?.AsString ?? "application/octet-stream";

        return (stream, fileName, contentType);
    }

    public async Task DeleteAsync(string gridFsId, CancellationToken ct = default)
    {
        var objectId = new ObjectId(gridFsId);
        await _gridFsBucket.DeleteAsync(objectId, ct);
    }
}