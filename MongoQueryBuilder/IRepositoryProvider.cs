using System;

namespace MongoQueryBuilder
{
    public interface IRepositoryProvider
    {
        IRepository<TModel, TQueryBuilder> CreateRepository<TModel, TQueryBuilder>(RepositoryConfiguration config)
            where TQueryBuilder : class, IQueryBuilder<TModel>
            where TModel : class;
    }
}

