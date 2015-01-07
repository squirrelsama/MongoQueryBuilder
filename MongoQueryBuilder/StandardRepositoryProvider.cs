using System;
using MongoDB.Driver;

namespace MongoQueryBuilder
{
    public class StandardRepositoryProvider : IRepositoryProvider
    {
        public IRepository<TModel, TQueryBuilder> CreateRepository<TModel, TQueryBuilder>(RepositoryConfiguration config)
            where TQueryBuilder : class, IQueryBuilder<TModel>
            where TModel : class
        {
            var server = MongoServer.Create(config.ConnectionString);
            var database = server.GetDatabase(config.DatabaseName);
            var collection = database.GetCollection(config.CollectionName);
            return new StandardRepository<TModel, TQueryBuilder>
            {
                Collection = collection,
                Config = config,
                QueryBuildery = new QueryBuildery()
            };
        }
    }
}

