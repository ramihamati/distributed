using MassTransit.Serialization;
using Newtonsoft.Json;

namespace ComX.Infrastructure.Distributed.Outbox
{
    public class EventSerializer : IEventSerializer
    {
        public object Deserialize(string body, Type eventType)
        {
            // because messages mostly are interfaces, we are using GreenPipes from masstransit
            // to deserialize the object
            using StringReader textReader = new(body);
            using JsonTextReader jsonReader = new(textReader);

            return JsonMessageSerializer
                .Deserializer.Deserialize(jsonReader, eventType);
        }

        public string Serialize<T>(T @event)
        {
            return JsonConvert.SerializeObject(@event);
        }
    }
}
