using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace ComX.Infrastructure.Distributed.Outbox;

public class AutoMapperService : IOutboxTransformer
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AutoMapperService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public TTransformed Transform<TSource, TTransformed>(TSource source)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        IMapper mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        return mapper.Map<TSource, TTransformed>(source);
    }
}
