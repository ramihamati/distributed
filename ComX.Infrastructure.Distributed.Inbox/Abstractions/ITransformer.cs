namespace ComX.Infrastructure.Distributed.Inbox;
public interface ITransformer
{
    TTransformed Transform<TSource, TTransformed>(TSource source);
}
