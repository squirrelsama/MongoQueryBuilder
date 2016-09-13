using System;
using System.Reflection;
using Castle.DynamicProxy;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Driver;


namespace MongoQueryBuilder.Tests
{
    public class ContainsConvention : IQueryBuilderMethodConvention
    {
        public Func<Type, MethodInfo, bool>[] Criteria =
            {
                (t,m) => m.Name.EndsWith("Has"),
                (t,m) => m.GetParameters().Length == 1,
                (t,m) => t.GetProperties()
                    .Any(p => p.Name == ExtractPropertyName(m.Name)),
                (t,m) => t.GetProperties()
                    .First(p => p.Name == ExtractPropertyName(m.Name))
                    .PropertyType.GetElementType() == m.GetParameters().First().ParameterType
                };

        public bool Matches(Type type, MethodInfo method)
        {
            return Criteria.All(i => i(type, method));
        }

        public IMongoQuery GenerateQueryComponent(IInvocation invocation)
        {
            var propertyName = ContainsConvention.ExtractPropertyName(invocation.Method.Name);
            return Query.And(
                Query.Exists(propertyName),
                Query.NE(propertyName, BsonNull.Value),
                Query.In(propertyName, invocation.Arguments.Select(BsonValue.Create).ToArray()));
        }
        public UpdateBuilder GenerateUpdateComponent(IInvocation invocation)
        {
            return null;
        }
        public static string ExtractPropertyName(string methodName)
        {
            return Regex.Replace(methodName, "Contains$", "");
        }
    }

    public class ByConvention : IQueryBuilderMethodConvention
    {
        public Func<Type, MethodInfo, bool>[] Criteria =
            {
                (t,m) => m.Name.StartsWith("By"),
                (t,m) => m.GetParameters().Length == 1,
                (t,m) => t.GetProperties()
                    .Any(p => p.Name == ExtractPropertyName(m.Name)),
                    (t,m) => t.GetProperties()
                    .First(p => p.Name == ExtractPropertyName(m.Name))
                    .PropertyType == m.GetParameters().First().ParameterType
                };

        public bool Matches(Type type, MethodInfo method)
        {
            return this.Criteria.All(i => i(type, method));
        }

        public IMongoQuery GenerateQueryComponent(IInvocation invocation)
        {
            return Query.EQ(
                ByConvention.ExtractPropertyName(invocation.Method.Name),
                BsonValue.Create(invocation.Arguments[0]));
        }
        public UpdateBuilder GenerateUpdateComponent(IInvocation invocation)
        {
            return null;
        }
        public static string ExtractPropertyName(string methodName)
        {
            return Regex.Replace(methodName, "^By", "");
        }
    }
}

