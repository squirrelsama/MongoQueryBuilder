using System;
using Castle.DynamicProxy;
using System.Reflection;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoQueryBuilder.Exceptions;
using System.Linq;

namespace MongoQueryBuilder.Infrastructure
{
    public static class QueryInterceptor
    {
        public static readonly ISet<MethodInfo> GenericQueryBuilderMethods 
            = new HashSet<MethodInfo>(typeof(IQueryBuilder<>).GetMethods());
    }
    public class QueryInterceptor<TModel> : IInterceptor
        where TModel : class
    {
        public StandardQueryExecutor<TModel> QueryBuilder { get; set; }
        public IntermediateQueryDataContainer QueryData { get; set; }
        public QueryInterceptor(StandardQueryExecutor<TModel> queryBuilder, IntermediateQueryDataContainer queryData)
        {
            this.QueryBuilder = queryBuilder;
            this.QueryData = queryData;
        }
        public void Intercept(IInvocation invocation)
        {
            if(!TryExecuteQuery(invocation))
                TryMatchConvention(invocation);
            invocation.Proceed();
        }
        public bool TryExecuteQuery(IInvocation invocation)
        {
            if(QueryInterceptor.GenericQueryBuilderMethods.Contains(invocation.Method))
            {
                invocation.Method.Invoke(this.QueryBuilder, invocation.Arguments);
                return true;
            }
            return false;
        }
        public bool TryMatchConvention(IInvocation invocation)
        {
            var conventionMatch = MethodConventionParser.Instance.Parse(invocation);
            this.QueryData.QueryComponents.Add(conventionMatch.Item1);
            this.QueryData.UpdateComponents.Add(conventionMatch.Item2);
            return true;
        }
    }
}

