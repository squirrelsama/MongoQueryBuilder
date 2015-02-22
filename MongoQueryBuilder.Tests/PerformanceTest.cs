using System;
using System.Linq;
using NUnit.Framework;
using System.Diagnostics;
using MongoDB.Driver;

namespace MongoQueryBuilder.Tests
{
    [TestFixture]
    public class PerformanceTest
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
        public void ItQueriesAHundredTimesInLessThanASecond()
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

            var totalWatch = Stopwatch.StartNew();

            Enumerable.Range(0,100).ToList().ForEach(_ => 
                Assert.True(repo.Builder()
                    .ByName("foo")
                    .ChildCompaniesContains(1)
                    .Queryable()
                    .Any()));

            totalWatch.Stop();
            Assert.LessOrEqual(totalWatch.ElapsedMilliseconds, 1000, 
                "Either your machine is slow, or we need a different performance expectation.");
        }
    }
}

