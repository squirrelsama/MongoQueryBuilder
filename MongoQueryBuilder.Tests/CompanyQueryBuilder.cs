using System;
using MongoDB.Driver;

namespace MongoQueryBuilder.Tests
{
    public class Company
    {
        public int Id {get;set;}
        public string Name {get;set;}
        public int[] ChildCompanies {get;set;}
        public double[] Loc{get;set;}
    }


    public interface ICompanyQueryBuilder : IQueryBuilder<Company>
    {
        ICompanyQueryBuilder ByName(string name);
        ICompanyQueryBuilder ChildCompaniesContains(int childCompanyId);
    }

    public static class CompanyRepo
    {
        public static IRepository<Company,ICompanyQueryBuilder> CreateRepo(RepositoryConfiguration config = null)
        {
            var provider = new StandardRepositoryProvider();
            return provider.CreateRepository<Company, ICompanyQueryBuilder>(
                config ?? new RepositoryConfiguration
                {
                    CollectionName = "companies",
                    DatabaseName = "testdata",
                    ConnectionString = "mongodb://localhost",
                    SafeModeSetting = SafeMode.True
                }, typeof(ICompanyQueryBuilder), typeof(ByConvention), typeof(ContainsConvention));
        }
    }
}

