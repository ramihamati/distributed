using System.Reflection;

namespace ComX.Infrastructure.Distributed.Inbox;

public class ReflectiveGenericConsumerAbstractFactory
{
    private const BindingFlags _flags = BindingFlags.Public | BindingFlags.Instance;
    private readonly IGenericConsumerAbstractFactory _consumerAbstractFactory;

    public ReflectiveGenericConsumerAbstractFactory(
        IGenericConsumerAbstractFactory consumerAbstractFactory)
    {
        _consumerAbstractFactory = consumerAbstractFactory;
    }

    public object Create(Type eventType)
    {
        Type genericConsumerFactoryType = typeof(IGenericConsumerFactory<>).MakeGenericType(eventType);

        MethodInfo consumerAbstractFactoryCreateMethod = _consumerAbstractFactory
                      .GetType()
                      .GetMethod(nameof(IGenericConsumerAbstractFactory.Create), _flags)
                      ?? throw new Exception("Could not get method Create by reflection");

        MethodInfo methodInfo = consumerAbstractFactoryCreateMethod
                                           .MakeGenericMethod(eventType);

        object genericConsumerFactory = methodInfo
               .Invoke(_consumerAbstractFactory, Array.Empty<object>())
               ?? throw new Exception("Could not create the generic consumer factory");

        return genericConsumerFactory;
    }
}
