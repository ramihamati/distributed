namespace ComX.Infrastructure.Distributed.Outbox;

public class TransformerService : IOutboxTransformerService
{
    private readonly Dictionary<Type, Type> _mapped;
    private readonly IOutboxTransformer _transformer;

    public TransformerService(
        Dictionary<Type, Type> mapped,
        IOutboxTransformer transformer)
    {
        _mapped = mapped;
        this._transformer = transformer;
    }

    public TTarget Transform<TSource, TTarget>(TSource source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        KeyValuePair<Type, Type> kvp = _mapped.FirstOrDefault(r => r.Key == typeof(TSource) && r.Value == typeof(TTarget));

        if (kvp.Key is null)
        {
            throw new InvalidOperationException($"No map registered for source {typeof(TSource).Name} to target {typeof(TTarget).Name}");
        }

        return _transformer.Transform<TSource, TTarget>(source);
    }
}
