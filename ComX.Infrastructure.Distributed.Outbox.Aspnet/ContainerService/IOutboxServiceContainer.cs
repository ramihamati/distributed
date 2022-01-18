using System;

namespace ComX.Infrastructure.Distributed.Outbox;

public interface IOutboxServiceContainer
{
    string Name { get; }
    IServiceProvider ServiceProvider { get; }
}
