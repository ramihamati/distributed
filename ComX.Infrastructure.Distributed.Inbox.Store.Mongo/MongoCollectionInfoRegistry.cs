namespace ComX.Infrastructure.Distributed.Inbox.Store.Mongo;
using MongoDB.Driver;

public class MongoCollectionInfoRegistry
{
    private readonly List<MongoCollectionInfo> collectionInfos;

    public MongoCollectionInfoRegistry()
    {
        collectionInfos = new List<MongoCollectionInfo>();
    }

    public MongoCollectionInfo? GetCollectionInfo<TModel>()
    {
        return collectionInfos.Find(r=> r.ModelType == typeof(TModel));
    }

    public void RegisterCollectionInfo(Type modelType, string dbName, string collectionName)
    {
        if (string.IsNullOrWhiteSpace(dbName))
        {
            throw new ArgumentException("The mongo database name cannot be empty");
        }
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new ArgumentException("The mongo collection name cannot be empty");
        }
        if (collectionInfos.Any(r=> r.ModelType == modelType))
        {
            throw new Exception($"A mongo registration for the type {modelType.FullName} is already present");
        }
        if (collectionInfos.Any(r=> string.Equals( r.CollectionName ,collectionName, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new Exception($"The collection name {collectionName} was already used");
        }

        collectionInfos.Add(new MongoCollectionInfo(modelType, dbName, collectionName));
    }
}
