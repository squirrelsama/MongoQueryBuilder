using System;
using System.Linq;
using NUnit.Framework;
using MongoDB.Driver;
using MongoQueryBuilder.Exceptions;

namespace MongoQueryBuilder.Tests
{
    [TestFixture]
    public class WrapperTests
    {
        [Test]
        public void ItDoesNotCallTheWrapperOverInMemoryOperations()
        {
            var wrapperCalls = 0;
            Action<Action> wrapper = action =>
            {
                wrapperCalls++;
                action();
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
            repo.Save(new Company
                {
                    Id = 1,
                    Name = "bar"
                });
            repo.Save(new Company
                {
                    Id = 2,
                    Name = "foo",
                    ChildCompanies = new [] { 1 }
                });
            repo.Save(new Company
                {
                    Id = 3,
                    Name = "bar",
                    ChildCompanies = new [] { 1 }
                });

            var queryableCalls = 0;
            var enumerableCalls = 0;
            repo
                .Builder()
                .ByName("bar")
                .Queryable(c =>
                {
                    queryableCalls++;
                    return c;
                })
                .Where(i => i.Id == 1)
                .Select(i =>
                {
                    enumerableCalls++;
                    return i;
                })
                .ToList();
            Assert.AreEqual(1, wrapperCalls);
            Assert.AreEqual(2, queryableCalls);
            Assert.AreEqual(1, enumerableCalls);
        }

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

