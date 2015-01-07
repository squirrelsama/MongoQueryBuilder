using System;
using NUnit.Framework;
using MongoDB.Driver;
using System.Reflection;
using MongoQueryBuilder.Exceptions;

namespace MongoQueryBuilder.Tests
{
    [TestFixture]
    public class BootstrappingTests
    {
        public class Company
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public interface IBadCompanyQueryBuilder : IQueryBuilder<Company>
        {
            IBadCompanyQueryBuilder ByName(int value);
        }

        [Test]
        public void ItThrowsWhenYourQueryBuilderInterfaceMismatchesYourModel()
        {
            Assert.Throws<NoMatchingMethodConventionException>(() =>
            {
                var provider = new StandardRepositoryProvider();
                var repo = provider.CreateRepository<Company, IBadCompanyQueryBuilder>(
                           new RepositoryConfiguration
                    {
                        CollectionName = "companies",
                        DatabaseName = "testdata",
                        ConnectionString = "mongodb://localhost",
                        SafeModeSetting = SafeMode.True
                    }, typeof(IBadCompanyQueryBuilder), typeof(ByConvention));
            });
        }
        [Test]
        public void ItThrowsWhenNoConventionsMatchYourQueryBuilderMethod()
        {
            Assert.Throws<NoMatchingMethodConventionException>(() =>
                {
                    var provider = new StandardRepositoryProvider();
                    var repo = provider.CreateRepository<Company, IBadCompanyQueryBuilder>(
                        new RepositoryConfiguration
                        {
                            CollectionName = "companies",
                            DatabaseName = "testdata",
                            ConnectionString = "mongodb://localhost",
                            SafeModeSetting = SafeMode.True
                        }, typeof(IBadCompanyQueryBuilder));
                });
        }
        [Test]
        public void ItDoesNotThrowWhenConventionsDontMatchAnything()
        {
            Assert.DoesNotThrow(() =>
                {
                    var provider = new StandardRepositoryProvider();
                    var repo = provider.CreateRepository<Company, IBadCompanyQueryBuilder>(
                        new RepositoryConfiguration
                        {
                            CollectionName = "companies",
                            DatabaseName = "testdata",
                            ConnectionString = "mongodb://localhost",
                            SafeModeSetting = SafeMode.True
                        }, typeof(ByConvention));
                });
        }
    }
}

