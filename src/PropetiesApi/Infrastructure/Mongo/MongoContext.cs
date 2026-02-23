using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using PropertiesService.Domain;
using System;

namespace PropertiesService.Infrastructure.Mongo;

public class MongoContext
{
    private readonly IMongoDatabase _database;

    public MongoContext(IConfiguration config)
    {
        static string GetRequired(IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Missing required configuration key: {key}");
            }

            return value;
        }

        var databaseName = GetRequired(config, "MONGO_INITDB_DATABASE");
        var username = GetRequired(config, "MONGO_INITDB_ROOT_USERNAME");
        var password = GetRequired(config, "MONGO_INITDB_ROOT_PASSWORD");
        var host = GetRequired(config, "MONGO_HOST");
        var port = GetRequired(config, "MONGO_PORT");
        var authSource = GetRequired(config, "MONGO_AUTH_SOURCE");

        var connectionString =
            $"mongodb://{Uri.EscapeDataString(username)}:{Uri.EscapeDataString(password)}@{host}:{port}/?authSource={authSource}";

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<Property> Properties =>
        _database.GetCollection<Property>("properties");
}