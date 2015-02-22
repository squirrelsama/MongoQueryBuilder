using System;
using MongoDB.Driver;

namespace MongoQueryBuilder
{
    public interface IRepository<TModel, TQueryBuilder>
        where TModel : class
    {
        MongoCollection Collection { get; }
        bool Save(TModel item);
        TQueryBuilder Builder();
    }
}

