using MongoDB.Driver;
using Polly;
using Polly.Retry;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxMongoManager
{
    private readonly IMongoClient _mongoClient;

    public OutboxMongoManager(IOutboxMongoSettings settings)
    {
        string connectionString = settings.ConnectionString;

        if (!connectionString.Contains("connectTimeoutMS") && settings.ConnectionTimeout.TotalMilliseconds > 0)
        {
            AddParameter(
             ref connectionString,
             "connectTimeoutMS",
             settings.ConnectionTimeout.TotalMilliseconds.ToString());
        }

        if (!connectionString.Contains("keepAlive"))
        {
            AddParameter(
             ref connectionString,
             "keepAlive",
             "true");
        }
        if (!connectionString.Contains("autoReconnect"))
        {
            AddParameter(
             ref connectionString,
             "autoReconnect",
             "true");
        }

        if (!connectionString.Contains("socketTimeoutMS") && settings.ConnectionTimeout.TotalMilliseconds > 0)
        {
            AddParameter(
             ref connectionString,
             "connectTimeoutMS",
             settings.ConnectionTimeout.TotalMilliseconds.ToString());
        }

        _mongoClient = new MongoClient(connectionString);
    }

    private static void AddParameter(ref string connectionString, string key, string value)
    {
        if (connectionString.Contains("?"))
        {
            connectionString += $"&{key}={value}";
        }
        else
        {
            if (!connectionString.EndsWith("/"))
            {
                connectionString += "/";
            }
            connectionString += $"?{key}={value}";
        }

    }

    public bool HasDatabase(string dbName)
    {
        return _mongoClient.ListDatabaseNames().ToList().Contains(dbName);
    }

    public IMongoCollection<TDocument> GetCollection<TDocument>(string dbName, string collectionName)
    {
        // creating the database on the first request if the db does not exists
        IMongoDatabase database = this.GetDatabase(dbName);

        // creating the collection on the first request if the collcetion does not exist
        // do not use this approach. In a cluster scenarion where transactions are used the collection must exist

        RetryPolicy retryPolicy
            = Policy.Handle<Exception>()
                    .WaitAndRetry(
                        retryCount: 5,
                        sleepDurationProvider: _ => TimeSpan.FromMilliseconds(1000));

        PolicyResult result = retryPolicy.ExecuteAndCapture(() =>
        {
            if (!database.ListCollectionNames().ToList().Contains(collectionName))
            {
                database.CreateCollection(collectionName);
            }
        });

        return database.GetCollection<TDocument>(collectionName,
            new MongoCollectionSettings
            {
                WriteConcern = WriteConcern.Acknowledged,
                ReadConcern = ReadConcern.Majority,
                ReadPreference = ReadPreference.PrimaryPreferred
            });
    }

    private IMongoDatabase GetDatabase(string dbName)
    {
        return _mongoClient.GetDatabase(dbName, new MongoDatabaseSettings
        {
            WriteConcern = WriteConcern.Acknowledged,
            ReadConcern = ReadConcern.Majority,
            ReadPreference = ReadPreference.PrimaryPreferred,
        });
    }
}
