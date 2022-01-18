using System;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class SampleSqlSettings : ISqlSettings
    {
        public string ConnectionString => throw new NotImplementedException();

        public int MaxRetryCount => throw new NotImplementedException();

        public TimeSpan MaxRetryDelay => throw new NotImplementedException();
    }
}
