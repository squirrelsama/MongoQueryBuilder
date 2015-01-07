using System;
using System.Reflection;

namespace MongoQueryBuilder
{
    public interface IRepositoryProvider
    {
        IRepository<TModel, TQueryBuilder> CreateRepository<TModel, TQueryBuilder>(RepositoryConfiguration config, params Assembly[] assemblies)
            where TQueryBuilder : class, IQueryBuilder<TModel>
            where TModel : class;
    }
}

