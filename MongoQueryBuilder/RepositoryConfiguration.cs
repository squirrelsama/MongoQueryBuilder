using System;
using MongoDB.Driver;

namespace MongoQueryBuilder
{
    public class RepositoryConfiguration
    {
        public RepositoryConfiguration()
        {
            this.CustomWrapper = action => action();
        }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public SafeMode SafeModeSetting { get; set; }
        public Action<Action> CustomWrapper { get; set; }
    }
}