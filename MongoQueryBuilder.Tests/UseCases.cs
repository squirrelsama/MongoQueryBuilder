using System;
using NUnit.Framework;
using System.Collections.Generic;

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
        ICompanyQueryBuilder HasChildCompany(int childCompanyId);
    }

    [TestFixture]
    public class UseCases
    {
        [Test]
        public void ItDoesTheThing()
        {
            var repo = new QueryBuildery().Create<ICompanyQueryBuilder, Company>("", "", "");



            var child = new Company { Name = "Test One" };
            Assert.DoesNotThrow(() => repo.Save(child));
            Assert.DoesNotThrow(() => repo.Save(new Company 
            { 
                Name = "Test Two", ChildCompanies = new [] { child.Id }
            }));
            Assert.AreEqual(1, repo.ByName("Test Two").HasChildCompany(child.Id).GetAll().Count);
        }
    }
}


