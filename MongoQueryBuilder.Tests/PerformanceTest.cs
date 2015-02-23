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
        [Test]
        public void ItQueriesAHundredTimesInLessThanASecond()
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

            var totalWatch = Stopwatch.StartNew();

            Enumerable.Range(0,100).ToList().ForEach(_ => 
                Assert.True(repo.Builder()
                    .ByName("foo")
                    .ChildCompaniesContains(1)
                    .Queryable(q => q)
                    .Any()));

            totalWatch.Stop();
            Assert.LessOrEqual(totalWatch.ElapsedMilliseconds, 1000, 
                "Either your machine is slow, or we need a different performance expectation.");
        }
    }
}

