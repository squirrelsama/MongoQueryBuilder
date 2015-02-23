using System;
using NUnit.Framework;
using MongoDB.Driver;
using MongoQueryBuilder.Exceptions;

namespace MongoQueryBuilder.Tests
{
    [TestFixture]
    public class ExceptionTests
    {
        [Test]
        public void ItThrowsUnsafeMongoOperationException()
        {
            var repo = CompanyRepo.CreateRepo();
            repo.Collection.Drop();

            Assert.Throws<UnsafeMongoOperationException>(() => repo.Builder().GetAll());
        }
    }
}

