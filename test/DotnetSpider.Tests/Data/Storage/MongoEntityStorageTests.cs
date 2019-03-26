using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Data;
using DotnetSpider.Data.Storage;
using DotnetSpider.Data.Storage.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace DotnetSpider.Tests.Data.Storage
{
    public class MongoEntityStorageTests
    {
        /// <summary>
        /// 测试芒果数据库存储数据成功
        /// 1. 数据库名是否正确
        /// 2. Collection 是否正确
        /// 3. 数据存储是否正确
        /// </summary>
        [Fact]
        public async Task Store_Should_Success()
        {
            var serviceProvider = Mock.Of<IServiceProvider>();

            var mongoCollection = new Mock<IMongoCollection<BsonDocument>>();
            
            var mongoDatabase = new Mock<IMongoDatabase>();
            mongoDatabase.Setup(d =>
                    d.GetCollection<BsonDocument>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                .Returns(mongoCollection.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient.Setup(d => d.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
                .Returns(mongoDatabase.Object);
            
            var mongoEntityStorage = new MongoEntityStorage(mongoClient.Object);
            
            var tableMetadata = new TableMetadata {Schema = new Schema("db", "table")};
            
            var dataFlowContext = new DataFlowContext(serviceProvider);
            dataFlowContext.Add("table", tableMetadata);
            dataFlowContext.AddItem("table", new object[] {new Dictionary<string, object> {{"Name", "Value"}}});
            
            var result = await mongoEntityStorage.HandleAsync(dataFlowContext);
            
            Assert.Equal(DataFlowResult.Success, result);
        }


        [Fact]
        public async Task Store_Empty_Should_Success()
        {
            var serviceProvider = Mock.Of<IServiceProvider>();
            var mongoClient = new Mock<IMongoClient>();
            var mongoEntityStorage = new MongoEntityStorage(mongoClient.Object);
            
            var dataFlowContext = new DataFlowContext(serviceProvider);
            var result = await mongoEntityStorage.HandleAsync(dataFlowContext);
            Assert.Equal(DataFlowResult.Success, result);
        }
    }
}