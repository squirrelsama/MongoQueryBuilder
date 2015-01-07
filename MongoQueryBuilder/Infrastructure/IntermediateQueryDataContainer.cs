using System;
using MongoDB.Driver;
using System.Collections.Generic;
using MongoDB.Driver.Builders;

namespace MongoQueryBuilder.Infrastructure
{
    public class IntermediateQueryDataContainer
    {
        // TODO implement min/max (e.g. Geo) with QueryDocument
        // public QueryDocument QueryDocument { get; set; }
        public List<IMongoQuery> QueryComponents { get; set; }
        public List<UpdateBuilder> UpdateComponents { get; set; }

        public IntermediateQueryDataContainer()
        {
            //this.QueryDocument = new QueryDocument();
            this.QueryComponents = new List<IMongoQuery>();
            this.UpdateComponents = new List<UpdateBuilder>();
        }
    }
}

