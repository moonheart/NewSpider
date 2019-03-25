using System;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Data;
using DotnetSpider.Data.Parser;
using DotnetSpider.Data.Storage;
using DotnetSpider.Downloader;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Sample.samples
{
    public class WholeSiteSpider
    {
        public static void Run1()
        {
            var services = new ServiceCollection();
            services.AddDotnetSpider(builder =>
            {
                builder.UseConfiguration();
                builder.UseSerilog();
                builder.UseStandalone();
                builder.UseDefaultDownloaderAllocator();
            });
            var factory = services.BuildServiceProvider().GetRequiredService<ISpiderFactory>();
            var spider = factory.Create<Spider>();

            spider.Id = Guid.NewGuid().ToString("N"); // 设置任务标识
            spider.Name = "博客园全站采集"; // 设置任务名称
            spider.Speed = 1; // 设置采集速度, 表示每秒下载多少个请求, 大于 1 时越大速度越快, 小于 1 时越小越慢, 不能为0.
            spider.Depth = 3; // 设置采集深度
            spider.DownloaderOptions.Type = DownloaderType.HttpClient; // 使用普通下载器, 无关 Cookie, 干净的 HttpClient
            spider.AddDataFlow(new DataParser
            {
                Selectable = context => context.GetSelectable(ContentType.Html),
                CanParse = DataParser.RegexCanParse("cnblogs\\.com"),
                Follow = DataParser.XpathFollow(".")
            }).AddDataFlow(new ConsoleStorage()); // 控制台打印采集结果
            spider.AddRequests("http://www.cnblogs.com/"); // 设置起始链接
            spider.RunAsync(); // 启动
        }

        public static Task Run2()
        {
            var services = new ServiceCollection();
            services.AddDotnetSpider(builder =>
            {
                builder.UseConfiguration();
                builder.UseSerilog();
                builder.UseStandalone();
                builder.UseDefaultDownloaderAllocator();
            });
            var factory = services.BuildServiceProvider().GetRequiredService<ISpiderFactory>();
            var options = factory.GetOptions();
            var spider = factory.Create<Spider>();
            spider.Id = Guid.NewGuid().ToString("N"); // 设置任务标识
            spider.Name = "博客园全站采集"; // 设置任务名称
            spider.Speed = 1; // 设置采集速度, 表示每秒下载多少个请求, 大于 1 时越大速度越快, 小于 1 时越小越慢, 不能为0.
            spider.Depth = 3; // 设置采集深度
            spider.DownloaderOptions.Type = DownloaderType.HttpClient; // 使用普通下载器, 无关 Cookie, 干净的 HttpClient
            spider.AddDataFlow(new CnblogsDataParser()).AddDataFlow(new MongoEntityStorage(options.ConnectionString));
            spider.AddRequests("http://www.cnblogs.com/"); // 设置起始链接
            return spider.RunAsync(); // 启动
        }

        class CnblogsDataParser : DataParser
        {
            public CnblogsDataParser()
            {
                CanParse = RegexCanParse("cnblogs\\.com");
                Follow = XpathFollow(".");
            }

            protected override Task<DataFlowResult> Parse(DataFlowContext context)
            {
                var response = context.GetResponse();
                context.AddItem("URL", response.Request.Url);
                context.AddItem("Title", context.GetSelectable().XPath(".//title").GetValue());
                return Task.FromResult(DataFlowResult.Success);
            }
        }
    }
}