using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;

namespace MongoQueryBuilder
{
    public class StandardRepository<TModel, TQueryBuilder> : IRepository<TModel, TQueryBuilder>
        where TQueryBuilder : class, IQueryBuilder<TModel>
        where TModel : class
    {
        public RepositoryConfiguration Config { get; set; }
        public MongoCollection Collection { get; set; }
        public QueryBuildery QueryBuildery { get; set; }

        public bool Save(TModel item)
        {
            var result = this.Collection.Save(item, this.Config.SafeModeSetting);
            return result.Ok;
        }
        public TQueryBuilder Builder()
        {
            return this.QueryBuildery.CreateProxyInterceptor<TModel, TQueryBuilder>(this.Config, this.Collection);
        }
        public TModel Queryable(Func<IQueryable<TModel>, TModel> func)
        {
            return func(this.Collection.AsQueryable<TModel>());
        }
        public IEnumerable<TModel> Queryable(Func<IQueryable<TModel>, IQueryable<TModel>> func)
        {
            return func(this.Collection.AsQueryable<TModel>()).AsEnumerable();
        }
    }
}

