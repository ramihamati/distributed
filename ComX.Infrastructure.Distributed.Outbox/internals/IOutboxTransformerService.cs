namespace ComX.Infrastructure.Distributed.Outbox;

public interface IOutboxTransformerService
{
    TTarget Transform<TSource, TTarget>(TSource source);
}
