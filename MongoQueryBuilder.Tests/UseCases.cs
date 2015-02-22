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

        public class Company
        {
            public int Id {get;set;}
            public string Name {get;set;}
            public int[] ChildCompanies {get;set;}
        }


        public interface ICompanyQueryBuilder : IQueryBuilder<Company>
        {
            ICompanyQueryBuilder ByName(string name);
            ICompanyQueryBuilder ChildCompaniesContains(int childCompanyId);
        }

        [Test]
        public void ItThrowsWhenItCannotConvertCriteriaToQueryable()
        {
            var provider = new StandardRepositoryProvider();
            var repo = provider.CreateRepository<Company, ICompanyQueryBuilder>(
                new RepositoryConfiguration
                {
                    CollectionName = "companies",
                    DatabaseName = "testdata",
                    ConnectionString = "mongodb://localhost",
                    SafeModeSetting = SafeMode.True
                }, typeof(ICompanyQueryBuilder), typeof(ByConvention), typeof(ContainsConvention));
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
                .Queryable()
                .Where(i => i.Name.Split(',').Count() == 0)
                .Count());
        }

        [Test]
        public void ItExposesMongoQueryable()
        {
            var provider = new StandardRepositoryProvider();
            var repo = provider.CreateRepository<Company, ICompanyQueryBuilder>(
                new RepositoryConfiguration
                {
                    CollectionName = "companies",
                    DatabaseName = "testdata",
                    ConnectionString = "mongodb://localhost",
                    SafeModeSetting = SafeMode.True
                }, typeof(ICompanyQueryBuilder), typeof(ByConvention), typeof(ContainsConvention));

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
                .Queryable()
                .Where(i => i.Name == "bar")
                .Count());

            Assert.AreEqual(2, repo.Builder()
                .Queryable()
                .Where(i => i.Name == "bar")
                .Count());

            Assert.AreEqual(1, repo.Builder()
                .ByName("bar")
                .Queryable()
                .Where(i => i.ChildCompanies.Any())
                .Count());
        }

        [Test]
        public void ItQueriesSuccessfullyWithTwoConventions()
        {
            var provider = new StandardRepositoryProvider();
            var repo = provider.CreateRepository<Company, ICompanyQueryBuilder>(
                new RepositoryConfiguration
                {
                    CollectionName = "companies",
                    DatabaseName = "testdata",
                    ConnectionString = "mongodb://localhost",
                    SafeModeSetting = SafeMode.True
                }, typeof(ICompanyQueryBuilder), typeof(ByConvention), typeof(ContainsConvention));
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
            var provider = new StandardRepositoryProvider();
            var repo = provider.CreateRepository<Company, ICompanyQueryBuilder>(
                new RepositoryConfiguration
                {
                    CollectionName = "companies",
                    DatabaseName = "testdata",
                    ConnectionString = "mongodb://localhost",
                    SafeModeSetting = SafeMode.True
                }, typeof(ICompanyQueryBuilder), typeof(ByConvention), typeof(ContainsConvention));
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


