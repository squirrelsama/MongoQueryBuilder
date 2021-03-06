﻿using System;
using Castle.DynamicProxy;
using System.Reflection;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoQueryBuilder.Exceptions;
using System.Linq;

namespace MongoQueryBuilder.Infrastructure
{
    public class QueryInterceptor<TModel> : IInterceptor
        where TModel : class
    {
        public static readonly ISet<string> QueryBuilderMethodNames 
            = new HashSet<string>(typeof(IQueryBuilder<TModel>)
                .GetMethods()
                .Select(i => i.Name));

        public StandardQueryExecutor<TModel> QueryBuilder { get; set; }
        public IntermediateQueryDataContainer QueryData { get; set; }
        public MethodConventionParser Parser { get; set; }
        public QueryInterceptor(StandardQueryExecutor<TModel> queryBuilder, MethodConventionParser parser, IntermediateQueryDataContainer queryData)
        {
            this.QueryBuilder = queryBuilder;
            this.QueryData = queryData;
            this.Parser = parser;
        }
        public void Intercept(IInvocation invocation)
        {
            try
            {
                if (!TryExecuteQuery(invocation))
                {
                    TryMatchConvention(invocation);
                    invocation.ReturnValue = invocation.Proxy;
                }
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }
        public bool TryExecuteQuery(IInvocation invocation)
        {
            if(QueryBuilderMethodNames.Contains(invocation.Method.Name))
            {
                invocation.ReturnValue = invocation.Method.Invoke(this.QueryBuilder, invocation.Arguments);
                return true;
            }
            return false;
        }
        public bool TryMatchConvention(IInvocation invocation)
        {
            var conventionMatch = this.Parser.Parse(invocation);
            if(conventionMatch.Item1 != null)
                this.QueryData.QueryComponents.Add(conventionMatch.Item1);
            if(conventionMatch.Item2 != null)
                this.QueryData.UpdateComponents.Add(conventionMatch.Item2);
            return true;
        }
    }
}

