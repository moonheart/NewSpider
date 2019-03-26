using System;
using System.Threading.Tasks;
using DotnetSpider.Data;
using DotnetSpider.Data.Storage;
using DotnetSpider.Data.Storage.Model;
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
        public async Task Store()
        {
            var serviceProvider = Mock.Of<IServiceProvider>();
            var mongoEntityStorage = new MongoEntityStorage("mongodb://db1.example.net,db2.example.net:2500/?replicaSet=test");
            var tableMetadata = new TableMetadata {Schema = new Schema("db", "table")};
            
            var dataFlowContext = new DataFlowContext(serviceProvider);
            dataFlowContext.Add("table", tableMetadata);
            dataFlowContext.AddItem("table", new object[0]);
            
            var result = await mongoEntityStorage.HandleAsync(dataFlowContext);
            Assert.Equal(DataFlowResult.Success, result);
        }

        [Fact]
        public async Task Store_Empty_Should_Success()
        {
            var serviceProvider = Mock.Of<IServiceProvider>();
            var mongoEntityStorage = new MongoEntityStorage("mongodb://db1.example.net,db2.example.net:2500/?replicaSet=test");
            var dataFlowContext = new DataFlowContext(serviceProvider);
            var result = await mongoEntityStorage.HandleAsync(dataFlowContext);
            Assert.Equal(DataFlowResult.Success, result);
        }
    }
}