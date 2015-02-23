using System;
using NUnit.Framework;
using MongoDB.Driver;
using MongoQueryBuilder.Exceptions;

namespace MongoQueryBuilder.Tests
{
    [TestFixture]
    public class WrapperTests
    {
        [Test]
        public void ItCallsTheWrapper()
        {
            var called = false;
            Action<Action> wrapper = action =>
            {
                called = true;
                action();
            };
            var repo = CompanyRepo.CreateRepo(new RepositoryConfiguration
            {
                CollectionName = "companies",
                DatabaseName = "testdata",
                ConnectionString = "mongodb://localhost",
                SafeModeSetting = SafeMode.True,
                CustomWrapper = wrapper
            });
            repo.Collection.Drop();

            Assert.Throws<UnsafeMongoOperationException>(() => repo.Builder().GetAll());
            Assert.IsFalse(called);
            Assert.IsEmpty(repo.Builder().GetAll(true));
            Assert.IsTrue(called);
        }
    }
}

