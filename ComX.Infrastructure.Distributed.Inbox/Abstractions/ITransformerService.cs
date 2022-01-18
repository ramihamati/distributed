namespace ComX.Infrastructure.Distributed.Inbox;

public interface ITransformerService
{
    bool HasMap<TSource>();
    object Transform<TSource>(TSource source);
}
