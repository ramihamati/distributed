The timer worker library allows you to define one or multiple workers that start on a configurable timer and executes a worker process
The worker process has to inherit the `IWorkerProcess` interface. It’s not mandatory to register it in `IServiceCollection` because it will automatically be registered if not found.

Internally the library will create a hosted service `BackgroundWorker<TProcess>` which will be the actual background worker. In turn it will call the process methods to execute.

```cs
public class MyWorkerTimerProcess : IWorkerProcess
    {
        public ServiceInjected Service { get; }
        
        public MyWorkerTimerProcess(ServiceInjected service)
        {
            Service = service;
        }
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public Task ProcessAsync()
        {
            return Task.CompletedTask;
        }
    }
```

To register the worker process
```cs
services.AddTransient<ServiceInjected>();
services.AddWorkerProgramabilityTimer<MyWorkerTimerProcess>(new Configuration(TimeSpan.FromSeconds(100)));
```
or
```cs
services.AddTransient<ServiceInjected>();
services.AddWorkerProgramabilityTimer<MyWorkerTimerProcess>(
    (sp) => new Configuration(TimeSpan.FromSeconds(100)));
```           
or 
```cs
services.AddTransient<ServiceInjected>;();
services.AddWorkerProgramabilityTimer<Configuration , MyWorkerTimerProcess>();
```

The configuration must implement the interface `IConfigurationTimer` and it is injected as a service if method 3 of registration is used (meaning you can have constructor injection)

```cs
public class Configuration : IConfigurationTimer
    {
        private readonly TimeSpan workerPeriod;
        public TimeSpan WorkerPeriod =&gt; workerPeriod;

        public Configuration(TimeSpan timeSpan)
        {
            this.workerPeriod = timeSpan;
        }
    }
```

**Note:**
The types of workers can be extended by creating new types that implement `IBackgroundProcessor` and register them accordingly in the library<