using Microsoft.Extensions.DependencyInjection;

namespace ComX.Infrastructure.Distributed.Inbox.Store.Urf
{
    public class UrfStoreProvider : IStoreProvider
    {
        private readonly IServiceProvider _services;

        public UrfStoreProvider(IServiceProvider services)
        {
            _services = services;
        }

        public IStore<TModel>? GetStore<TModel>() where TModel : class
        {
            return _services.GetRequiredService<IStore<TModel>>();
        }
    }
}