using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Data.Storage.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DotnetSpider.Data.Storage
{
    public class MongoEntityStorage : StorageBase
    {
        private readonly MongoClient _client;

        private readonly ConcurrentDictionary<string, IMongoDatabase> _cache =
            new ConcurrentDictionary<string, IMongoDatabase>();

        public MongoEntityStorage(string connectionString)
        {
            _client = new MongoClient(connectionString);
        }

        protected override async Task<DataFlowResult> Store(DataFlowContext context)
        {
            var items = context.GetItems();
            if (items == null || items.Count == 0)
            {
                return DataFlowResult.Success;
            }

            foreach (var item in items)
            {
                var tableMetadata = (TableMetadata) context[item.Key];

                if (!_cache.ContainsKey(tableMetadata.Schema.Database))
                {
                    _cache.TryAdd(tableMetadata.Schema.Database, _client.GetDatabase(tableMetadata.Schema.Database));
                }

                var db = _cache[tableMetadata.Schema.Database];
                var collection = db.GetCollection<BsonDocument>(tableMetadata.Schema.Table);

                var bsonDocs = new List<BsonDocument>();
                foreach (var data in (List<Dictionary<string, string>>) item.Value)
                {
                    if (!data.ContainsKey("creation_time"))
                    {
                        data.Add("creation_time", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                    }

                    bsonDocs.Add(BsonDocument.Create(data));
                }

                await collection.InsertManyAsync(bsonDocs);
            }

            return DataFlowResult.Success;
        }
    }
}