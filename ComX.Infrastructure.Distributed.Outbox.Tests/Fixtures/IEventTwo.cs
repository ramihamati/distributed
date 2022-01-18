using System;
using System.Collections.Generic;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public interface IEventTwo 
    {
        public Guid ReferenceId { get; }
        public string ReferenceType { get; }
    }
}
