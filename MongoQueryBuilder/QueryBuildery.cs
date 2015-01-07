using System;
using System.Linq;
using Castle.DynamicProxy;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System.Reflection;
using MongoDB.Driver;

namespace MongoQueryBuilder
{
    public interface IRepository<TQueryBuilder,TModel>
        where TQueryBuilder : IQueryBuilder<TModel>
    {
        bool Save(TModel item);
        bool Delete();
        TModel ById(int id);
        TQueryBuilder Build();
    }
    public interface IQueryBuilder<TModel>
        where TModel : class
    {
        List<TModel> GetAll();
        TModel GetOne();
    }

    public class QueryBuilder<T>
    {
        public List<IMongoQuery> QueryComponents = new List<IMongoQuery>();
    }

    public class NoMatchingMethodConventionException : Exception 
    {
        public IInvocation AttemptedInvocation { get; set; }
        public NoMatchingMethodConventionException(IInvocation attemptedInvocation)
        { 
            this.AttemptedInvocation = attemptedInvocation;
        }
    } 

    public class MethodConventionParser
    {
        public IMethodConvention Parse(IInvocation invocation)
        {
            return Assembly.GetExecutingAssembly()
                .DefinedTypes
                .Where(i => i.IsAssignableFrom(typeof(IMethodConvention)))
                .Select(i => (IMethodConvention)Activator.CreateInstance(i))
                .FirstOrDefault(i => i.Matches(invocation));
            
        }
        public interface IMethodConvention
        {
            bool Matches(IInvocation invocation);
            IMongoQuery GenerateComponent(IInvocation invocation);
        }

        public class HasConvention : IMethodConvention
        {
            public bool Matches(IInvocation invocation)
            {
                if(!invocation.Method.Name.StartsWith("Has"))
                    return false;
                if(invocation.Arguments.Length != 1)
                    return false;
                if(!(invocation.Arguments[0] is string))
                    return false;
                return true;
            }
            public IMongoQuery GenerateComponent(IInvocation invocation)
            {
                throw new NotImplementedException();
            }
        }


        public class ByConvention : IMethodConvention
        {
            public bool Matches(IInvocation invocation)
            {
                if(!invocation.Method.Name.StartsWith("By"))
                    return false;
                if(invocation.Arguments.Length != 1)
                    return false;
                if(!(invocation.Arguments[0] is string))
                    return false;
                return true;
            }
            public IMongoQuery GenerateComponent(IInvocation invocation)
            {
                throw new NotImplementedException();
                //return Query.
            }
        }
    }

    public class QueryInterceptor<T> : IInterceptor
    {
        public MethodConventionParser Parser = new MethodConventionParser();
        public QueryBuilder<T> MongoState = new QueryBuilder<T>();
        public void Intercept(IInvocation invocation)
        {
            var match = this.Parser.Parse(invocation);
            if(match == null)
                throw new NoMatchingMethodConventionException(invocation);
            this.MongoState.QueryComponents.Add(match.GenerateComponent(invocation));
            invocation.Proceed();
        }

    }

    public class QueryBuildery
    {
        private static readonly ProxyGenerator _generator = new ProxyGenerator();
        public T Create<T,K>(string connectionString, string databaseName, string collectionName)
            where T : class, IQueryBuilder<K>
            where K : class
        {
            var interceptor = new QueryInterceptor<K>();
            var proxy = _generator.CreateInterfaceProxyWithoutTarget<T>(interceptor);
            return proxy;
        }
    }
}


