===============
Register Events
===============

.. toctree::
   :maxdepth: 2
   :titlesonly:
   :hidden:

.. role:: yaml(code)
   :language: yaml 

.. role:: cs(code)
   :language: cs 

Each event must be registered with a name. This name is saved in the in the stored :cs:`IIntegrationMessageLog` (which is builtin :cs:`IntegrationMessageLog` 
if you are using the default repository). The name is used by the :cs:`IOutboxWorker` to match the event body to the target for deserialization.

.. code-block:: cs

    services.AddOutboxService(cfg =>
    {
        cfg.RegisterEvents(reg =>
        {
            reg.RegisterMessage<IEventOne>("IEventDocumentStored");
            reg.RegisterMessage<IEventTwo>("IEventPersistDocumentsCompleted");
            reg.RegisterMessage<IEventThree>("IEventPrepareDocumentsFailed");
        });
        // other code commented to be explaine in other sections
    });

The register method has another overload which allows you to specify what entity should it use to save the event in the database.
The default entity is :cs:`IntegrationMessageLog`

**Overloads**

.. code-block:: cs

    public void RegisterMessage<TMessageType>(string name);
    public void RegisterMessage<TMessageType, TMessageLog>(string name)
        where TMessageLog : class, IIntegrationMessageLog;

When we add a custom integration message log, we tell the service that when it wants to store the event in the database,
it should use that specific message and we should provide a repository for it. The custom entity has the same properties as 
:cs:`IntegrationMessageLog`, it only helps to determine a new repository for a different event.

You can implement the new message in 2 ways:

.. code-block:: cs

    public class CustomMessageLog : IIntegrationMessageLog
    {
        public Guid Id { get; set; }

        public string MessageBody { get; set; }

        public OutboxStatus Status { get; set; }

        public string MessageTypeName { get; set; }

        public DateTime CreatedAt { get;  set; }

        // In case the message failed to be published, set an offset for the next attempt to 
        // allow the system to recover and to pick up next messages
        public DateTime? LastAttemptDate { get; set; } = DateTime.UtcNow.Subtract(TimeSpan.FromDays(365));

        public int RetryCount { get; set; } = 0;

        public byte[] Timestamp { get; set; }

        public string LastError { get; set; }
        public DateTime? LockUntil { get; set; }

        public CustomMessageLog()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }

or

.. code-block:: cs

    public class CustomMessageLog : IntegrationMessageLog
    {
    }