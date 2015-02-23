using System;
using System.Collections.Generic;
using System.Linq;
using MongoQueryBuilder.Infrastructure;
namespace MongoQueryBuilder
{
    public interface IQueryBuilder<TModel> : IQueryBuilder
        where TModel : class
    {
        long DeleteAll(bool allowWithoutCriteria = false);
        bool DeleteOne(bool allowWithoutCriteria = false);
        long UpdateAll(bool allowWithoutCriteria = false);
        bool UpdateOne(bool allowWithoutCriteria = false);
        List<TModel> GetAll(bool allowWithoutCriteria = false);
        List<TModel> GetSome(int limit, bool allowWithoutCriteria = false);
        TModel GetOne();
        TModel Queryable(Func<IQueryable<TModel>, TModel> func);
        IEnumerable<TModel> Queryable(Func<IQueryable<TModel>, IQueryable<TModel>> func);
    }
}

