using System;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class EventTwo : IEventTwo
    {
        public Guid ReferenceId { get; set; }

        public string ReferenceType { get; set; }
    }
}
