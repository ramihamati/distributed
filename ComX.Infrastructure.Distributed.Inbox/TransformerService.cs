using System.Reflection;

namespace ComX.Infrastructure.Distributed.Inbox;

public class TransformerService : ITransformerService
{
    private readonly Dictionary<Type, Type> _mapped;
    private readonly ITransformer _transformer;

    public TransformerService(
        Dictionary<Type, Type> mapped,
        ITransformer transformer)
    {
        _mapped = mapped;
        this._transformer = transformer;
    }

    public bool HasMap<TSource>()
    {
        return _mapped.ContainsKey(typeof(TSource));
    }

    public object Transform<TSource>(TSource source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (!_mapped.ContainsKey(typeof(TSource)))
        {
            throw new InvalidOperationException($"No map registered for source {typeof(TSource).FullName}");
        }

        KeyValuePair<Type, Type> kvp = _mapped.FirstOrDefault(r => r.Key == typeof(TSource));

        MethodInfo mInfo = _transformer
            .GetType()
            .GetMethod(nameof(ITransformer.Transform), BindingFlags.Public | BindingFlags.Instance)
            ?.MakeGenericMethod(kvp.Key, kvp.Value)
            ?? throw new Exception("Error reflecting on method Transform of the Transformer");

        return mInfo.Invoke(_transformer, new object[] { source })
            ?? throw new Exception("Error getting the transformed object");
    }
}
