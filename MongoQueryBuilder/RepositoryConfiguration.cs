using System;
using MongoDB.Driver;

namespace MongoQueryBuilder
{
    public class RepositoryConfiguration
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public SafeMode SafeModeSetting { get; set; }
    }
}   