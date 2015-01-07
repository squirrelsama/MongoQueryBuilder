using System;
using NUnit.Framework;
using System.Collections.Generic;
using MongoDB.Driver;

namespace MongoQueryBuilder.Tests
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

    [TestFixture]
    public class UseCases
    {
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
                });
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
            Assert.AreEqual(0, repo.Query()
                .ChildCompaniesContains(0)
                .GetAll()
                .Count);
        }
    }
}


