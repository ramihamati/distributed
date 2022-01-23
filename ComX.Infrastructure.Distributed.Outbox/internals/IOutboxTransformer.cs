namespace ComX.Infrastructure.Distributed.Outbox;

public interface IOutboxTransformer
{
    TTransformed Transform<TSource, TTransformed>(TSource source);
}
