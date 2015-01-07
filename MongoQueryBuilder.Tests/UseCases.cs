using System;
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
        public void ItDoesTheThing()
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

            Assert.AreEqual(1, repo.Query()
                .ByName("Test One")
                .GetAll()
                .Count);
            Assert.AreEqual(2, repo.Query()
                .GetAll(true)
                .Count);
            Assert.AreEqual(1, repo.Query()
                .ChildCompaniesContains(1)
                .GetAll()
                .Count);
            Assert.AreEqual(1, repo.Query()
                .ByName("Test Two")
                .ChildCompaniesContains(1)
                .GetAll()
                .Count);
            Assert.AreEqual(0, repo.Query()
                .ChildCompaniesContains(0)
                .GetAll()
                .Count);
            Assert.AreEqual(0, repo.Query()
                .ByName("Test One")
                .ChildCompaniesContains(1)
                .GetAll()
                .Count);
        }

    }
}


