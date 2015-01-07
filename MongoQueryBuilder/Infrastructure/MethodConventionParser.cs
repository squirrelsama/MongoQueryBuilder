using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using MongoDB.Driver;
using MongoQueryBuilder.Exceptions;
using Castle.DynamicProxy;
using MongoDB.Driver.Builders;

namespace MongoQueryBuilder.Infrastructure
{
    public struct QueryBuilderMetadata
    {
        public Type GenericTypeOfQueryBuilder;
        public MethodInfo QueryBuilderMethod;
    }
    public class MethodConventionParser
    {
        public MethodConventionParser(params Assembly[] assemblies)
        {
            foreach (var entry in assemblies)
            {
                this.LoadConventionsFromAssembly(entry);
                this.LoadQueryBuildersFromAssembly(entry);
            }
            this.CreateConventionDictionary();
        }

        public QueryBuilderMetadata[] AllQueryBuilderMetadata = { };
        public IQueryBuilderMethodConvention[] AllQueryBuilderConventionDefinitions = { };
        public Dictionary<MethodInfo, IQueryBuilderMethodConvention> ConventionDictionary;

        public MethodConventionParser LoadQueryBuildersFromAssembly(Assembly ass, bool createConventionDictionary = true)
        {
            this.AllQueryBuilderMetadata = ass.DefinedTypes
                .Where(i => 
                    typeof(IQueryBuilder).IsAssignableFrom(i))
                .SelectMany(queryBuilderType => queryBuilderType.GetMethods()
                    .Select(method => new QueryBuilderMetadata
                    {
                        GenericTypeOfQueryBuilder = ExtractParentGenericType(queryBuilderType),
                        QueryBuilderMethod = method
                    }))
                .Where(i => i.GenericTypeOfQueryBuilder != null)
                .Concat(this.AllQueryBuilderMetadata)
                .Distinct()
                .ToArray();
            return this;
        }

        public Type ExtractParentGenericType(Type t)
        {  
            var genericInterface = t.GetInterfaces()
                .FirstOrDefault(i => i.GenericTypeArguments.Length > 0);
            if (genericInterface == null)
                return null;
            return genericInterface.GenericTypeArguments[0];
        }
        public MethodConventionParser LoadConventionsFromAssembly(Assembly ass, bool createConventionDictionary = true)
        {
            this.AllQueryBuilderConventionDefinitions = ass.DefinedTypes
                .Where(i => typeof(IQueryBuilderMethodConvention).IsAssignableFrom(i))
                .Select(i => (IQueryBuilderMethodConvention)Activator.CreateInstance(i))
                .Concat(this.AllQueryBuilderConventionDefinitions)
                .Distinct()
                .ToArray();
            return this;
        }

        public MethodConventionParser CreateConventionDictionary()
        {
            this.ConventionDictionary = this.AllQueryBuilderMetadata
                .Select(queryBuilderMetadata => new
                {
                    queryBuilderMethod = queryBuilderMetadata.QueryBuilderMethod,
                    convention = this.AllQueryBuilderConventionDefinitions.FirstOrDefault(convention => 
                            convention.Matches(queryBuilderMetadata.GenericTypeOfQueryBuilder, queryBuilderMetadata.QueryBuilderMethod))
                })
                .ToDictionary(i => i.queryBuilderMethod, i => i.convention);
            var badQueryBuilderMethod = this.ConventionDictionary.FirstOrDefault(i => i.Value == null);
            if (badQueryBuilderMethod.Key != null && badQueryBuilderMethod.Value != null)
                throw new NoMatchingMethodConventionException(badQueryBuilderMethod.Key);
            return this;
        }

        public Tuple<IMongoQuery, UpdateBuilder> Parse(IInvocation invocation)
        {
            var convention = this.ConventionDictionary[invocation.Method];
            var query = convention.GenerateQueryComponent(invocation);
            var update = convention.GenerateUpdateComponent(invocation);
            return Tuple.Create(query, update);
        }
    }
}

