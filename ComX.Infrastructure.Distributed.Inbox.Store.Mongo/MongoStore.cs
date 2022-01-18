namespace ComX.Infrastructure.Distributed.Inbox.Store.Mongo;

using ComX.Infrastructure.Distributed.Inbox;
using MongoDB.Driver;

public class MongoStore<TModel> : IStore<TModel>
    where TModel : class
{
    private readonly MongoManager _mongoManager;
    private readonly MongoCollectionInfoRegistry _registry;

    public MongoStore(
        MongoManager mongoManager,
        MongoCollectionInfoRegistry registry)
    {
        _mongoManager = mongoManager;
        _registry = registry;
    }

    public async Task SaveAsync(TModel model)
    {
        MongoCollectionInfo collectionInfo 
            = _registry.GetCollectionInfo<TModel>()
            ?? throw new NullReferenceException($"Could not find {nameof(MongoCollectionInfo)} for the type {typeof(TModel).FullName}");

        IMongoCollection<TModel> collection =  _mongoManager.GetCollection<TModel>(
            collectionInfo.DbName,
            collectionInfo.CollectionName);

        await collection.InsertOneAsync(model);
    }
}
