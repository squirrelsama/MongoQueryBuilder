using System;
using MongoDB.Driver;

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
        public TQueryBuilder Query()
        {
            return this.QueryBuildery.CreateProxyInterceptor<TModel, TQueryBuilder>(this.Config, this.Collection);
        }
    }
}

