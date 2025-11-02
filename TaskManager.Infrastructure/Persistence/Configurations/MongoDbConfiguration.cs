namespace TaskManager.Infrastructure.Persistence.Configurations;

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;

public static class MongoDbConfiguration
{
    public static void ConfigureMongoDB()
    {
        var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

        if (!BsonClassMap.IsClassMapRegistered(typeof(Entity)))
        {
            BsonClassMap.RegisterClassMap<Entity>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(e => e.Id);
                cm.MapProperty(e => e.CreatedAt)
                    .SetElementName("created_at")
                    .SetSerializer(new DateTimeSerializer(DateTimeKind.Utc));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
        {
            BsonClassMap.RegisterClassMap<User>(cm =>
            {
                cm.AutoMap();
                cm.MapProperty(u => u.Email).SetElementName("email");
                cm.MapProperty(u => u.Name).SetElementName("name");
                cm.MapProperty(u => u.Role).SetElementName("role");
                cm.MapProperty(u => u.HashedPassword).SetElementName("hashed_password");
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(TaskItem)))
        {
            BsonClassMap.RegisterClassMap<TaskItem>(cm =>
            {
                cm.AutoMap();
                cm.MapProperty(t => t.Title).SetElementName("title");
                cm.MapProperty(t => t.Description).SetElementName("description");
                cm.MapProperty(t => t.Status).SetElementName("status");
                cm.MapProperty(t => t.Priority).SetElementName("priority");
                cm.MapProperty(t => t.DueDate).SetElementName("due_date");
                cm.MapProperty(t => t.Tags).SetElementName("tags");
                cm.MapProperty(t => t.UserId).SetElementName("user_id");
                cm.MapProperty(t => t.AssignedUserIds).SetElementName("assigned_users");
                cm.MapProperty(t => t.Attachments).SetElementName("attachments");
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(TaskAttachment)))
        {
            BsonClassMap.RegisterClassMap<TaskAttachment>(cm =>
            {
                cm.AutoMap();
                cm.MapProperty(a => a.Id).SetElementName("id");
                cm.MapProperty(a => a.FileName).SetElementName("file_name");
                cm.MapProperty(a => a.FileSize).SetElementName("file_size");
                cm.MapProperty(a => a.ContentType).SetElementName("content_type");
                cm.MapProperty(a => a.GridFsId).SetElementName("gridfs_id");
                cm.MapProperty(a => a.UploadedAt)
                    .SetElementName("uploaded_at")
                    .SetSerializer(new DateTimeSerializer(DateTimeKind.Utc));
            });
        }
    }
}