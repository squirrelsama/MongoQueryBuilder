using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoQueryBuilder.Exceptions;
using MongoQueryBuilder.Infrastructure;
using System.Reflection;

namespace MongoQueryBuilder.Tests
{
    [TestFixture]
    public class UseCases
    {
        [Test]
        public void ItThrowsWhenItCannotConvertCriteriaToQueryable()
        {
            var repo = CompanyRepo.CreateRepo();

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

            Assert.Throws<NotSupportedException>(() => repo.Builder()
                .Queryable(q => q
                    .Where(i => i.Name.Split(',').Count() == 0))
                .Count());
        }

        [Test]
        public void ItExposesMongoQueryable()
        {
            var repo = CompanyRepo.CreateRepo();

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

            Assert.AreEqual(2, repo
                .Queryable(q => q
                    .Where(i => i.Name == "bar"))
                .Count());

            Assert.IsNotNull(repo
                .Queryable(q => q
                    .First(i => i.Name == "bar")));

            Assert.AreEqual(2, repo.Builder()
                .Queryable(q => q
                    .Where(i => i.Name == "bar"))
                .Count());

            Assert.AreEqual(1, repo.Builder()
                .ByName("bar")
                .Queryable(q => q
                    .Where(i => i.ChildCompanies.Any()))
                .Count());

            Assert.IsNotNull(repo.Builder()
                .ByName("bar")
                .Queryable(q => q.First()));
        }

        [Test]
        public void ItQueriesSuccessfullyWithTwoConventions()
        {
            var repo = CompanyRepo.CreateRepo();
            repo.Collection.Drop();
            repo.Save(new Company
            {
                Id = 1,
                Name = "Test One"
            });
            repo.Save(new Company
            {
                Id = 2,
                Name = "Test Two",
                ChildCompanies = new [] { 1 }
            });

            Assert.AreEqual(1, repo.Builder()
                .ByName("Test One")
                .GetAll()
                .Count);
            Assert.AreEqual(2, repo.Builder()
                .GetAll(true)
                .Count);
            Assert.AreEqual(1, repo.Builder()
                .ChildCompaniesContains(1)
                .GetAll()
                .Count);
            Assert.AreEqual(1, repo.Builder()
                .ByName("Test Two")
                .ChildCompaniesContains(1)
                .GetAll()
                .Count);
            Assert.AreEqual(0, repo.Builder()
                .ChildCompaniesContains(0)
                .GetAll()
                .Count);
            Assert.AreEqual(0, repo.Builder()
                .ByName("Test One")
                .ChildCompaniesContains(1)
                .GetAll()
                .Count);
        }

        [Test]
        public void ItLimitsResultSetsWhenYouAskItTo()
        {
            var repo = CompanyRepo.CreateRepo();
            repo.Collection.Drop();
            repo.Save(new Company
            {
                Id = 1,
                Name = "foo"
            });
            repo.Save(new Company
            {
                Id = 2,
                Name = "foo",
            });

            Assert.AreEqual(2, repo.Builder()
                .ByName("foo")
                .GetSome(3)
                .Count);
            Assert.AreEqual(2, repo.Builder()
                .ByName("foo")
                .GetSome(2)
                .Count);
            Assert.AreEqual(1, repo.Builder()
                .ByName("foo")
                .GetSome(1)
                .Count);
            Assert.AreEqual(2, repo.Builder()
                .ByName("foo")
                .GetSome(0)
                .Count);
            Assert.AreEqual(2, repo.Builder()
                .ByName("foo")
                .GetSome(-1)
                .Count);
        }

    }
}


