using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Devday {
    class DataManager
    {
        AmazonDynamoDBClient _client;

        public DataManager()
        {
           _client = new AmazonDynamoDBClient( new AmazonDynamoDBConfig { 
               RegionEndpoint = RegionEndpoint.EUWest1
            } );
        }

        public async Task<IList<T>> GetAllAsync<T>( IEnumerable<ScanCondition> conditions )
        {
            using ( var dbcontext = new DynamoDBContext( _client ) )
            {
                var scan = dbcontext.ScanAsync<T>( conditions );

                return await scan.GetNextSetAsync();
            }           
        }
    }
}