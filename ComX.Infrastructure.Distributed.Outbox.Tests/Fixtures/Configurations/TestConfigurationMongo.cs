using System;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class TestConfigurationMongo : IOutboxMongoSettings
    {
        public string ConnectionString { get; set;}

        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromMinutes(1);

        public string DbName { get; set; }

        public string CollectionName { get; set; }

        public TestConfigurationMongo(
            string connectionString,
            string dbName,
            string collectionName)
        {
            ConnectionString = connectionString;
            DbName = dbName;
            CollectionName = collectionName;
        }
    }
}