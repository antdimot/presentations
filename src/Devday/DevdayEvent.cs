
using System;
using Amazon.DynamoDBv2.DataModel;

namespace Devday {

    [DynamoDBTable( "Devday_Events" )]
    public class DevdayEvent
    {
        [DynamoDBHashKey]
        public int EventID { get; set; }

        [DynamoDBRangeKey]
        public string City { get; set; }       

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime Date { get; set; }   
    }
}