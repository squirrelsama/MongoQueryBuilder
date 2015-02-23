using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoQueryBuilder.Exceptions;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using System.Linq;

namespace MongoQueryBuilder.Infrastructure
{
    public class StandardQueryExecutor<TModel> : IQueryBuilder<TModel>
        where TModel : class
    {
        public IntermediateQueryDataContainer QueryData { get; set; }
        public RepositoryConfiguration Config { get; set; }
        public MongoCollection Collection { get; set; }
        public StandardQueryExecutor(RepositoryConfiguration config, MongoCollection collection, IntermediateQueryDataContainer queryData)
        {
            this.Config = config;
            this.Collection = collection;
            this.QueryData = queryData;
        }

        public T CallWrapperAndReturn<T>(Func<T> func)
        {
            T result = default(T);
            this.Config.CustomWrapper(() => result = func());
            return result;
        }
        public T Evaluate<T>(bool allowWithoutCriteria, Func<IMongoQuery, UpdateBuilder, T> func)
        {
            if (!allowWithoutCriteria && !this.QueryData.QueryComponents.Any())
                throw new UnsafeMongoOperationException("Cannot implicitly DeleteAll with no criteria. See the default parameter.");
            var query = this.QueryData.QueryComponents.Any() 
                ? Query.And(this.QueryData.QueryComponents) 
                : null;
            var updates = Update.Combine(this.QueryData.UpdateComponents);
            return CallWrapperAndReturn(() => func(query, updates));
        }
        public long DeleteAll(bool allowWithoutCriteria = false)
        {
            var result = this.Evaluate(allowWithoutCriteria, (query,_) => 
                this.Collection.Remove(query, RemoveFlags.None, this.Config.SafeModeSetting));
            return result.DocumentsAffected;     
        }
        public bool DeleteOne(bool allowWithoutCriteria = false)
        {
            var result = this.Evaluate(allowWithoutCriteria, (query,_) => 
                this.Collection.Remove(query, RemoveFlags.Single, this.Config.SafeModeSetting));
            return result.Ok;
        }
        public long UpdateAll(bool allowWithoutCriteria = false)
        {
            if (this.QueryData.UpdateComponents.Count < 1)
                return 0;

            var result = this.Evaluate(allowWithoutCriteria, (query,updates) => 
                this.Collection.Update(query, updates, UpdateFlags.Multi, this.Config.SafeModeSetting));
            return result.DocumentsAffected;
        }
        public bool UpdateOne(bool allowWithoutCriteria = false)
        {
            if (this.QueryData.UpdateComponents.Count < 1)
                return false;

            var result = this.Evaluate(allowWithoutCriteria, (query,updates) => 
                this.Collection.Update(query, updates, UpdateFlags.None, this.Config.SafeModeSetting));
            return result.Ok;
        }
        public List<TModel> GetAll(bool allowWithoutCriteria = false)
        {
            return this.Evaluate(allowWithoutCriteria, (query,updates) => 
                this.Collection.FindAs<TModel>(query).ToList());
        }
        public List<TModel> GetSome(int limit, bool allowWithoutCriteria = false)
        {
            if (limit < 1)
                return GetAll(allowWithoutCriteria);

            return this.Evaluate(allowWithoutCriteria, (query,updates) => 
                this.Collection.FindAs<TModel>(Query.Null).SetLimit(limit).ToList());
        }
        public TModel GetOne()
        {
            return this.Evaluate(true, (query,updates) => 
                this.Collection.FindOneAs<TModel>(query));
        }

        public T QueryableReduce<T>(Func<IQueryable<TModel>, T> func)
        {
            return this.Evaluate(true, (query,updates) => func(query == null
                ? this.Collection.AsQueryable<TModel>()  
                : from item in this.Collection.AsQueryable<TModel>()
                    where query.Inject()
                    select item));
        }

        public TModel Queryable(Func<IQueryable<TModel>, TModel> func)
        {
            return this.Evaluate(true, (query,updates) => func(query == null
                ? this.Collection.AsQueryable<TModel>()  
                : from item in this.Collection.AsQueryable<TModel>()
                    where query.Inject()
                    select item));
        }
        public IEnumerable<TModel> Queryable(Func<IQueryable<TModel>, IQueryable<TModel>> func)
        {
            return this.Evaluate(true, (query,updates) => func(query == null
                ? this.Collection.AsQueryable<TModel>()  
                : from item in this.Collection.AsQueryable<TModel>()
                    where query.Inject()
                    select item)
                .AsEnumerable());
        }
    }
}