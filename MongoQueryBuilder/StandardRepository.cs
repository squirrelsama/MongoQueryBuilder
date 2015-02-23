using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;
using MongoQueryBuilder.Infrastructure;

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

    public class StandardRepository<TModel, TQueryBuilder> : IRepository<TModel, TQueryBuilder>
        where TQueryBuilder : class, IQueryBuilder<TModel>
        where TModel : class
    {
        public RepositoryConfiguration Config { get; set; }
        public MongoCollection Collection { get; set; }
        public QueryBuildery QueryBuildery { get; set; }

        public T CallWrapperAndReturn<T>(Func<T> func)
        {
            T result = default(T);
            this.Config.CustomWrapper(() => result = func());
            return result;
        }

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
            return this.CallWrapperAndReturn(() => func(this.Collection.AsQueryable<TModel>()));
        }
        public IEnumerable<TModel> Queryable(Func<IQueryable<TModel>, IQueryable<TModel>> func)
        {
            return this.CallWrapperAndReturn(() => func(this.Collection.AsQueryable<TModel>()).AsEnumerable());
        }
    }
}

