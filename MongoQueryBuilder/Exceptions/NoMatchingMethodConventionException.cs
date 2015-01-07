using System;
using System.Reflection;

namespace MongoQueryBuilder.Exceptions
{
    public class NoMatchingMethodConventionException : Exception 
    {
        public MethodInfo BadMethod { get; set; }
        public NoMatchingMethodConventionException(MethodInfo badMethod)
            : base("A QueryBuilder<T> defined a method which does not match any convention.")
        { 
            this.BadMethod = badMethod;
        }
    } 
}

