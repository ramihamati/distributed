using System;

namespace ComX.Infrastructure.Distributed.Outbox
{
    public interface IEventSerializer
    {
        public string Serialize<T>(T @event);
        public object Deserialize(string body, Type eventType);
    }
}
