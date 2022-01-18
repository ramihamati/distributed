﻿namespace ComX.Infrastructure.Distributed.Inbox;

public class NoopTransformer : ITransformer
{
    public TTransformed Transform<TSource, TTransformed>(TSource source)
    {
        // will not be called. Required for DI when no map is added
        throw new NotImplementedException();
    }
}
