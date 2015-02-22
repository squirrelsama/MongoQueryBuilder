using System;
using NUnit.Framework;
using MongoDB.Driver;

namespace MongoQueryBuilder.Tests
{
    [TestFixture]
    public class GeoTests
    {
        [Test]
        public void ItGeoQueriesEfficiently()
        {
            var repo = CompanyRepo.CreateRepo();

            repo.Collection.Drop();
            repo.Save(new Company
            {
                Id = 1,
                Name = "bar",
                Loc = new[]{100.0, 100.0}

            });
            repo.Save(new Company
            {
                Id = 2,
                Name = "foo",
                Loc = new[]{90.0, 90.0}
            });
            repo.Save(new Company
            {
                Id = 3,
                Name = "bar",
                Loc = new[]{0.0, 0.0}
            });
        }
    }
}

