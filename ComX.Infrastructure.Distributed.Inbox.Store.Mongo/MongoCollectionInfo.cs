namespace ComX.Infrastructure.Distributed.Inbox.Store.Mongo;

public class MongoCollectionInfo
{
    public Type ModelType { get; }

    public string DbName { get; } = string.Empty;

    public string CollectionName { get; } = string.Empty;

    public MongoCollectionInfo(
        Type modelType,
        string dbName,
        string collectionName)
    {
        ModelType = modelType;
        DbName = dbName;
        CollectionName = collectionName;
    }
}
