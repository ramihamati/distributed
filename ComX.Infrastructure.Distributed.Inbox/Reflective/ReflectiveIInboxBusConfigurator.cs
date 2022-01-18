using System.Reflection;

namespace ComX.Infrastructure.Distributed.Inbox;

public class ReflectiveIInboxBusConfigurator
{
    private const BindingFlags _flags = BindingFlags.Public | BindingFlags.Instance;
    private readonly IInboxBusConfigurator _inboxBusConfigurator;
    private readonly MethodInfo _registerMethod;

    public ReflectiveIInboxBusConfigurator(IInboxBusConfigurator inboxBusConfigurator)
    {
        _inboxBusConfigurator = inboxBusConfigurator;
        _registerMethod = inboxBusConfigurator
           .GetType()
           .GetMethod(nameof(IInboxBusConfigurator.RegisterConsumer), _flags)
           ?? throw new Exception("Could not get method RegisterConsumer by reflection");
    }

    public void RegisterConsumer(Type eventType)
    {
        MethodInfo typedRegisterMethod
                  = _registerMethod.MakeGenericMethod(eventType);

        typedRegisterMethod.Invoke(_inboxBusConfigurator, Array.Empty<object>());
    }
}
