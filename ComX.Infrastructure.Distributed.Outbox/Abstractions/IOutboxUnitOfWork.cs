namespace ComX.Infrastructure.Distributed.Outbox;

public interface IOutboxUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
