namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxException : Exception
{
    public OutboxException() : base()
    {
    }

    public OutboxException(string message) : base(message)
    {
    }

    public OutboxException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
