using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Downloader.Entity;
using DotnetSpider.MessageQueue;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotnetSpider.Downloader.Internal
{
    public class LocalDownloadCenter : AbstractDownloadCenter
    {
        public LocalDownloadCenter(IMessageQueue mq, IDownloaderAgentStore downloaderAgentStore,
            ILoggerFactory loggerFactory) : base(mq, downloaderAgentStore, loggerFactory)
        {
        }

        public override async Task<bool> AllocateAsync(AllotDownloaderMessage allotDownloaderMessage)
        {
            List<DownloaderAgent> agents = null;
            for (int i = 0; i < 50; ++i)
            {
                agents = await DownloaderAgentStore.GetAllListAsync();
                if (agents.Count <= 0)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    break;
                }
            }

            if (agents == null)
            {
                Logger.LogError("未找到活跃的下载器代理");
                return false;
            }

            // 保存节点选取信息
            await DownloaderAgentStore.AllocateAsync(allotDownloaderMessage.OwnerId, new[] {agents[0].Id});
            Logger.LogInformation("下载器代理分配成功");
            // 发送消息让下载代理器分配好下载器
            var message =
                $"{Framework.AllocateDownloaderCommand}|{JsonConvert.SerializeObject(allotDownloaderMessage)}";
            foreach (var agent in agents)
            {
                await Mq.PublishAsync(agent.Id, message);
            }

            return true;
        }

        public override async Task EnqueueRequests(string ownerId, IEnumerable<Request> requests)
        {
            // 本机下载中心只会有一个下载代理
            var agents = await DownloaderAgentStore.GetAllListAsync(ownerId);
            if (agents.Count <= 0)
            {
                Logger.LogError("未找到活跃的下载器代理");
            }

            var agent = agents[0];
            var json = JsonConvert.SerializeObject(requests);
            await Mq.PublishAsync(agent.Id, $"{Framework.DownloadCommand}|{json}");
        }
    }
}