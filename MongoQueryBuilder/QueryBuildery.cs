using System;
using System.Linq;
using Castle.DynamicProxy;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System.Reflection;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using MongoQueryBuilder.Exceptions;
using MongoQueryBuilder.Infrastructure;

namespace MongoQueryBuilder
{
    public class QueryBuildery
    {
        private static readonly ProxyGenerator _generator = new ProxyGenerator();
        public static Dictionary<Type, MethodInfo[]> MethodInfoDictionary = new Dictionary<Type, MethodInfo[]>();

        public MethodConventionParser Parser { get; set; }
        public QueryBuildery(MethodConventionParser parser)
        {
            this.Parser = parser;
        }
        public TQueryBuilder CreateProxyInterceptor<TModel, TQueryBuilder>(
                RepositoryConfiguration config, 
                MongoCollection collection)
            where TQueryBuilder : class, IQueryBuilder<TModel>
            where TModel : class
        {
            var customMethods = this.GetOrAddMethods<TQueryBuilder>();

            var sharedIntermediateQueryData = new IntermediateQueryDataContainer();
            var queryBuilder = new StandardQueryExecutor<TModel>(config, collection, sharedIntermediateQueryData);
            var interceptor = new QueryInterceptor<TModel>(queryBuilder, this.Parser, sharedIntermediateQueryData);
            var proxy = _generator.CreateInterfaceProxyWithoutTarget<TQueryBuilder>(interceptor);
            return proxy;
        }
        public MethodInfo[] GetOrAddMethods<T>()
        {
            var type = typeof(T);
            if (MethodInfoDictionary.ContainsKey(type))
                return MethodInfoDictionary[type];
            return MethodInfoDictionary[type] = type.GetMethods();
        }
    }
}


