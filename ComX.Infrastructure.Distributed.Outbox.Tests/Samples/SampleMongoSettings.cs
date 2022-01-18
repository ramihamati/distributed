using System;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class SampleMongoSettings : IOutboxMongoSettings
    {
        public string ConnectionString => throw new NotImplementedException();

        public TimeSpan ConnectionTimeout => throw new NotImplementedException();

        public string DbName => throw new NotImplementedException();

        public string CollectionName => throw new NotImplementedException();
    }
}
