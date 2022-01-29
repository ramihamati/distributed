.. _workerprocess:

==============
WORKER PROCESS
==============

.. toctree::
   :maxdepth: 2
   :titlesonly:
   :hidden:

.. role:: cs(code)
   :language: cs 

About
-----
A flexible framework for creating multiple types of workers. The library is designed so that the behaviour of the worker
is split from the action performed

Internals
---------

The worker uses two main components for configuring the process:

* :cs:`IBackgroundProcessor` is used to control the behaviour of the worker. 

It handles how the worker is being created, destroyed and it's used by the hosted service. 
The StartAsync() method will use the `IWorkerProcess.ProcessAsync` method to actually process.

.. code-block:: cs

    interface IBackgroundProcessor
    {
        public Task StartAsync(Func<Task> func);
        public Task StopAsync();
        public void Dispose();
    }


* :cs:`IWorkerProcess` encapsulates the actual logic performed by the worker iteration

Programability Timer
------------------------

The :cs:`BackgroundTimerProcessor` is an implementation of the BackgroundProcessor that uses a Timer to start the process on a period basis, 
but guarantees that at no time (in the same process) multiple threads are started. 
If the thread is activelly processing and the timer has to restart, it just stores an intent of continuing the work after the active task finishes.

Example:

.. code-block:: cs
   :caption: create a process to be run by the worker

    public class MyWorkerTimerProcess : IWorkerProcess
    {
        public SomeOtherService Service { get; }
        
        public MyWorkerTimerProcess(SomeOtherService service)
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

.. code-block:: cs
   :caption: register the worker and it's process

    services.AddTransient<SomeOtherService>();
    services.AddWorkerProgramabilityTimer<MyWorkerTimerProcess>(new Configuration(TimeSpan.FromSeconds(100)));


Overloads
----------

The worker process represents the unit of processing which encapsulates the processing logic of the business, disregarding the worker behaviour.

**Usages**
To register the worker process

.. code-block:: cs

    services.AddWorkerProgramabilityTimer<MyWorkerTimerProcess>(new Configuration(TimeSpan.FromSeconds(100)));
    services.AddWorkerProgramabilityTimer<MyWorkerTimerProcess>((sp) => new Configuration(TimeSpan.FromSeconds(100)));
    services.AddWorkerProgramabilityTimer<Configuration , MyWorkerTimerProcess>();

where

.. code-block:: cs

    public class Configuration : IConfigurationTimer
    {
        private readonly TimeSpan workerPeriod;
        public TimeSpan WorkerPeriod =&gt; workerPeriod;

        public Configuration(TimeSpan timeSpan)
        {
            this.workerPeriod = timeSpan;
        }
    }


New Behaviour Types
-------------------
The types of workers can be extended by creating new types that implement `IBackgroundProcessor` 

* create a new processor

.. code-block:: cs 

    public class MyProcessor : IBackgroundProcessor
    {
    }

* create an extension method and use the `BackgroundWorker<>`

.. code-block:: cs

    public static void AddWorkerProgramabilityTimer<TWorkerProcess>(
        this IServiceCollection services)
            where TWorkerProcess : class, IWorkerProcess
    {
        services.TryAddScoped<TWorkerProcess>();
        services.AddHostedService<BackgroundWorker<TWorkerProcess>>(sp =>
        {
            return new BackgroundWorker<TWorkerProcess>(
                backgroundWorker: new MyProcessor(),
                loggerFactory: sp.GetService<ILoggerFactory>(),
                processFactory: () => sp.CreateScope().ServiceProvider.GetRequiredService<TWorkerProcess>());
        });
    }