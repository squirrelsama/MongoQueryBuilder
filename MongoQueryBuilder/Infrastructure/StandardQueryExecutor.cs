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
        public long DeleteAll(bool allowWithoutCriteria = false)
        {
            if (!allowWithoutCriteria && !this.QueryData.QueryComponents.Any())
                throw new UnsafeMongoOperationException("Cannot implicitly DeleteAll with no criteria. See the default parameter.");

            var query = Query.And(this.QueryData.QueryComponents);
            var result = this.Collection.Remove(query, RemoveFlags.None, this.Config.SafeModeSetting);
            return result.DocumentsAffected;
        }
        public bool DeleteOne(bool allowWithoutCriteria = false)
        {
            if (!allowWithoutCriteria && !this.QueryData.QueryComponents.Any())
                throw new UnsafeMongoOperationException("Cannot implicitly DeleteOne with no criteria. See the default parameter.");

            var query = Query.And(this.QueryData.QueryComponents);
            var result = this.Collection.Remove(query, RemoveFlags.Single, this.Config.SafeModeSetting);
            return result.Ok;
        }
        public long UpdateAll(bool allowWithoutCriteria = false)
        {
            if (this.QueryData.UpdateComponents.Count < 1)
                return 0;

            if (!allowWithoutCriteria && !this.QueryData.QueryComponents.Any())
                throw new UnsafeMongoOperationException("Cannot implicitly UpdateAll with no criteria. See the default parameter.");

            var query = Query.And(this.QueryData.QueryComponents);
            var updates = Update.Combine(this.QueryData.UpdateComponents);
            var result = this.Collection.Update(query, updates, UpdateFlags.Multi, this.Config.SafeModeSetting);
            return result.DocumentsAffected;
        }
        public bool UpdateOne(bool allowWithoutCriteria = false)
        {
            if (this.QueryData.UpdateComponents.Count < 1)
                return false;

            if (!allowWithoutCriteria && !this.QueryData.QueryComponents.Any())
                throw new UnsafeMongoOperationException("Cannot implicitly UpdateOne with no criteria. See the default parameter.");

            var query = Query.And(this.QueryData.QueryComponents);
            var updates = Update.Combine(this.QueryData.UpdateComponents);
            var result = this.Collection.Update(query, updates, UpdateFlags.None, this.Config.SafeModeSetting);
            return true;
        }
        public List<TModel> GetAll(bool allowWithoutCriteria = false)
        {
            if (!allowWithoutCriteria && !this.QueryData.QueryComponents.Any())
                throw new UnsafeMongoOperationException("Cannot implicitly GetAll with no criteria. See the default parameter.");

            if (this.QueryData.QueryComponents.Any())
            {
                var query = Query.And(this.QueryData.QueryComponents);
                return this.Collection.FindAs<TModel>(query).ToList();
            }
            return this.Collection.FindAs<TModel>(Query.Null).ToList();

        }
        public TModel GetOne()
        {
            if (this.QueryData.QueryComponents.Any())
            {
                var query = Query.And(this.QueryData.QueryComponents);
                return this.Collection.FindOneAs<TModel>(query);
            }
            return this.Collection.FindOneAs<TModel>(Query.Null);
        }
        public IQueryable<TModel> Queryable()
        {
            if (this.QueryData.QueryComponents.Any())
            {
                var query = Query.And(this.QueryData.QueryComponents);
                return from item in this.Collection.AsQueryable<TModel>()
                    where query.Inject()
                    select item;
            }
            return this.Collection.AsQueryable<TModel>();
        }
    }
}

