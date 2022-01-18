using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using URF.Core.Abstractions;
using URF.Core.EF;

namespace ComX.Infrastructure.Distributed.Inbox.Store.Urf;

public class UrfStoreConfigurator
{
    private readonly ConfiguratorContext _context;

    public UrfStoreConfigurator(
        ConfiguratorContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Will try to add the service <see cref="IRepository{TEntity}"/> with impl <see cref="Repository{TEntity}"/>
    /// </summary>
    public UrfStoreConfigurator UseRepository<TModel>() where TModel : class
    {
        if (_context.Services.Any(r => r.ServiceType == typeof(IRepository<TModel>)))
        {
            throw new Exception($"You already registered a {typeof(IRepository<TModel>).FullName}");
        }

        _context.Services.TryAddScoped<IRepository<TModel>, Repository<TModel>>();
        _context.Services.TryAddScoped<Repository<TModel>, Repository<TModel>>();
        _context.Services.TryAddScoped<IStore<TModel>, UrfStore<TModel>>();
        return this;
    }

    /// <summary>
    /// Will try to add the service <see cref="IRepository{TEntity}"/> with impl <typeparamref name="TRepository"/>
    /// where the <typeparamref name="TRepository"/> is an implementation of <see cref="IRepository{TEntity}"/>
    /// </summary>
    public UrfStoreConfigurator UseRepository<TModel, TRepository>() where TModel : class
        where TRepository : class, IRepository<TModel>
    {
        if (_context.Services.Any(r => r.ServiceType == typeof(IRepository<TModel>)))
        {
            throw new Exception($"You already registered a {typeof(IRepository<TModel>).FullName}");
        }

        _context.Services.TryAddScoped<IRepository<TModel>, TRepository>();
        _context.Services.TryAddScoped<TRepository, TRepository>();
        _context.Services.TryAddScoped<IStore<TModel>, UrfStore<TModel>>();
        return this;
    }

    /// <summary>
    /// Will try to add the service <see cref="IRepository{TEntity}"/> with impl <typeparamref name="TRepository"/>
    /// where the <typeparamref name="TRepository"/> is an implementation of <see cref="IRepository{TEntity}"/>
    /// <para>Intstructing URF to use the provided TContext instead of default DbContext</para>
    /// </summary>
    public UrfStoreConfigurator UseRepositoryWithContext<TModel, TContext>() where TModel : class
        where TContext : DbContext
    {
        if (_context.Services.Any(r => r.ServiceType == typeof(IRepository<TModel>)))
        {
            throw new Exception($"You already registered a {typeof(IRepository<TModel>).FullName}");
        }

        _context.Services.TryAddScoped<IRepository<TModel>>(
            sp => new Repository<TModel>(sp.GetRequiredService<TContext>()));

        _context.Services.TryAddScoped<IStore<TModel>, UrfStore<TModel>>();
        return this;
    }
}
