namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxConcurrencyException : Exception
{
    public OutboxConcurrencyException() : base()
    {
    }

    public OutboxConcurrencyException(string message) : base(message)
    {
    }

    public OutboxConcurrencyException(string message, Exception innerException) : base(message, innerException)
    {
    }

}
