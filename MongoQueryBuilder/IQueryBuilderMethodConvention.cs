using System;
using Castle.DynamicProxy;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Reflection;

namespace MongoQueryBuilder
{
    public interface IQueryBuilderMethodConvention
    {
        bool Matches(Type type, MethodInfo method);
        IMongoQuery GenerateQueryComponent(IInvocation invocation);
        UpdateBuilder GenerateUpdateComponent(IInvocation invocation);
    }
}

