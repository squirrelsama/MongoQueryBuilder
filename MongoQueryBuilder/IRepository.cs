using System;
using MongoDB.Driver;
using System.Linq;

namespace MongoQueryBuilder
{
    public interface IRepository<TModel, TQueryBuilder>
        where TModel : class
    {
        MongoCollection Collection { get; }
        bool Save(TModel item);
        TQueryBuilder Builder();
        IQueryable<TModel> Queryable();
    }
}

