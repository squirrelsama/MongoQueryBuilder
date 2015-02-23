using System;
using MongoDB.Driver;
using System.Linq;
using System.Collections.Generic;

namespace MongoQueryBuilder
{
    public interface IRepository<TModel, TQueryBuilder>
        where TModel : class
    {
        MongoCollection Collection { get; }
        bool Save(TModel item);
        TQueryBuilder Builder();
        TModel Queryable(Func<IQueryable<TModel>, TModel> func);
        IEnumerable<TModel> Queryable(Func<IQueryable<TModel>, IQueryable<TModel>> func);
    }
}

