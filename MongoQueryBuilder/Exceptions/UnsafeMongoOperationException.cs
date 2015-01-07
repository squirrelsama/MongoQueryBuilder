using System;

namespace MongoQueryBuilder.Exceptions
{
    public class UnsafeMongoOperationException : Exception
    {
        public UnsafeMongoOperationException(string message) 
            : base(message)
        {
        }
    }
}

