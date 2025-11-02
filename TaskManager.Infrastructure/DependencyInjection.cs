namespace TaskManager.Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Authentication;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Persistence.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        MongoDbConfiguration.ConfigureMongoDB();

        var mongoUrl = configuration["MongoDB:ConnectionString"]
                       ?? throw new ArgumentNullException("MongoDB:ConnectionString");
        var databaseName = configuration["MongoDB:DatabaseName"]
                           ?? throw new ArgumentNullException("MongoDB:DatabaseName");

        var mongoClient = new MongoClient(mongoUrl);
        var database = mongoClient.GetDatabase(databaseName);

        services.AddSingleton<IMongoDatabase>(database);
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IAuthService, AuthService>();

        return services;
    }
}