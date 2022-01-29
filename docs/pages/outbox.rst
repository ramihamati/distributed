======
OUTBOX
======

.. https://www.sphinx-doc.org/en/master/usage/restructuredtext/directives.html
.. toctree::
   :maxdepth: 2
   :titlesonly:
   :hidden:

   outbox-service
   outbox-worker

The outbox library follows the outbox pattern where events are saved in the database and a sepparate worker reads the 
database and publishes the events to the broker.

This adds an increased resiliency layer while keeping a log in the database for analysis.

The outbox library offers 2 products:
* the outbox service : handles storing events in the database

* the outbox worker : handles reading and publishing

The library is modular:


With the outbox pattern we are decoupling the sending of the event from the broker by adding a database layer in between. 
The scope is to increase the resiliency by having the messages stored in the database before they are sent.

With the outbox library we are listening saving events in the database which are later pushed by the worker to the broker. 
For the moment there are no options to delete the messages after they are pushed.

The outbox library provides 2 services.
* the outbox worker (with the purpose of reading the database and sending the events)
* the outbox service (with the purpose of saving events to be sent in the database)

.. https://stackoverflow.com/questions/10870719/inline-code-highlighting-in-restructuredtext
.. role:: yaml(code)
   :language: yaml 

:yaml:`ComX.Infrastructure.Distributed.Outbox` : the basic functionality
:yaml:`ComX.Infrastructure.Distributed.Outbox.Aspnet`: dependency injection support
:yaml:`ComX.Infrastructure.Distributed.Outbox.Masstransit`: a masstransit implementation of the publisher
:yaml:`ComX.Infrastructure.Distributed.Outbox.Store.Mongo`: a mongo implementation of the store
:yaml:`ComX.Infrastructure.Distributed.Outbox.Store.Sql`: a sql implementation of the store


IntegrationMessageLog
---------------------

The events are serialized by default using Newtonsoft and stored using `IntegrationMessageLog`. This model
can change by providing a custom implementation.

.. code-block:: cs

    public class IntegrationMessageLog
    {
        public Guid Id { get; set; }
        public string MessageBody { get; set; }
        public Status Status { get; set; }
        public string MessageTypeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastAttemptDate { get; set; }
        public DateTime? LockUntil { get; set; }
        public int RetryCount { get; set; } = 0;
        public byte[] Timestamp { get; set; }
        public string LastError { get; set; }
        public IntegrationMessageLog()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }