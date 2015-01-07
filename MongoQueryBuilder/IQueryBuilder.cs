using System;
using System.Collections.Generic;

namespace MongoQueryBuilder
{
    public interface IQueryBuilder<TModel>
        where TModel : class
    {
        long DeleteAll(bool allowWithoutCriteria = false);
        bool DeleteOne(bool allowWithoutCriteria = false);
        long UpdateAll(bool allowWithoutCriteria = false);
        bool UpdateOne(bool allowWithoutCriteria = false);
        List<TModel> GetAll(bool allowWithoutCriteria = false);
        TModel GetOne();
    }
}

