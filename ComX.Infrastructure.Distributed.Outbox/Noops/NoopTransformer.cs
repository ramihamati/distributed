namespace ComX.Infrastructure.Distributed.Outbox;

public class NoopTransformer : IOutboxTransformer
{
    public TTransformed Transform<TSource, TTransformed>(TSource source)
    {
        // will not be called. Required for DI when no map is added
        throw new NotImplementedException();
    }
}
