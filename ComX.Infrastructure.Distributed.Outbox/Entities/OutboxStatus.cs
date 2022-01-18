namespace ComX.Infrastructure.Distributed.Outbox
{
    public enum OutboxStatus : short
    {
        NotPublished,
        Published,
        ErrorState
    }
}
