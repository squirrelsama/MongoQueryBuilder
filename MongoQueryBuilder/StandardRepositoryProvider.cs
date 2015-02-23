using System;
using MongoDB.Driver;
using MongoQueryBuilder.Infrastructure;
using System.Reflection;
using System.Linq;

namespace MongoQueryBuilder
{
    public interface IRepositoryProvider
    {
        IRepository<TModel, TQueryBuilder> CreateRepository<TModel, TQueryBuilder>(RepositoryConfiguration config, params Type[] types)
            where TQueryBuilder : class, IQueryBuilder<TModel>
            where TModel : class;

        IRepository<TModel, TQueryBuilder> CreateRepository<TModel, TQueryBuilder>(RepositoryConfiguration config, params Assembly[] assemblies)
            where TQueryBuilder : class, IQueryBuilder<TModel>
            where TModel : class;
    }

    public class StandardRepositoryProvider : IRepositoryProvider
    {
        public IRepository<TModel, TQueryBuilder> CreateRepository<TModel, TQueryBuilder>(RepositoryConfiguration config, params Type[] types)
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
                QueryBuildery = new QueryBuildery(new MethodConventionParser(types))
            };
        }
        public IRepository<TModel, TQueryBuilder> CreateRepository<TModel, TQueryBuilder>(RepositoryConfiguration config, params Assembly[] assemblies)
            where TQueryBuilder : class, IQueryBuilder<TModel>
            where TModel : class
        {
            return this.CreateRepository<TModel,TQueryBuilder>(
                config,
                assemblies.SelectMany(i => i.GetTypes()).ToArray());
        }
    }
}

