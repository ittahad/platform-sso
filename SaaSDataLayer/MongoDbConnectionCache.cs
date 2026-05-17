using MongoDB.Driver;

namespace SaaSDataLayer
{
    public static class MongoDbConnectionCache
    {
        private static readonly string DatabaseIdFormat = "{0}-{1}";

        private static readonly IDictionary<string, IMongoDatabase> cachedMongoDatabases = new Dictionary<string, IMongoDatabase>();

        public static void BuildCache(Tenant[] tenants)
        {
            cachedMongoDatabases.Clear();

            foreach (var tenant in tenants)
            {
                foreach (var vertical in tenant.Verticals)
                {
                    var stateDatabase = GetMongoDatabase(vertical.StateServerConnectionString, vertical.StateDatabaseName);

                    var stateDatabaseId = string.Format(DatabaseIdFormat, tenant.Id, vertical.Id);

                    cachedMongoDatabases.Add(stateDatabaseId, stateDatabase);
                }
            }
        }


        public static IMongoDatabase GetVerticalDataContext(string tenantId, string verticalId)
        {
            var databaseId = string.Format(DatabaseIdFormat, tenantId, verticalId);

            return cachedMongoDatabases[databaseId];
        }

        private static IMongoDatabase GetMongoDatabase(string databaseConnectionString, string databaseName)
        {
            var mongoUrl = new MongoUrl(databaseConnectionString);
            var mongoClientSettings = MongoClientSettings.FromUrl(mongoUrl);
            mongoClientSettings.RetryWrites = true;
            return new MongoClient(mongoClientSettings).GetDatabase(databaseName);
        }

        public class Tenant
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public Vertical[] Verticals { get; set; }
        }

        public class Vertical
        {
            public string Id { get; set; }
            public string StateDatabaseName { get; set; }
            public string StateServerConnectionString { get; set; }
        }
    }

}