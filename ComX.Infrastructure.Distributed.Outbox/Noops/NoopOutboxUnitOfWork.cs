namespace ComX.Infrastructure.Distributed.Outbox;

/// <summary>
/// NoOperation UnitOfWork required to override the built in unit of work. This class
/// is used when we are working with external DbContexts instead of an internal one, and transactional
/// commits are handled by the caller.
/// </summary>
public class NoopOutboxUnitOfWork : IOutboxUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
