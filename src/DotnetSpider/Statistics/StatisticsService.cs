using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using DotnetSpider.Core;
using DotnetSpider.MessageQueue;

namespace DotnetSpider.Statistics
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IMessageQueue _mq;

        public StatisticsService(IMessageQueue mq)
        {
            _mq = mq;
        }

        public async Task IncrementSuccessAsync(string ownerId)
        {
            await _mq.PublishAsync(DotnetSpiderConsts.StatisticsServiceTopic, $"Success|{ownerId}");
        }

        public async Task IncrementFailedAsync(string ownerId)
        {
            await _mq.PublishAsync(DotnetSpiderConsts.StatisticsServiceTopic, $"Failed|{ownerId}");
        }

        public async Task StartAsync(string ownerId)
        {
            await _mq.PublishAsync(DotnetSpiderConsts.StatisticsServiceTopic, $"Start|{ownerId}");
        }

        public async Task ExitAsync(string ownerId)
        {
            await _mq.PublishAsync(DotnetSpiderConsts.StatisticsServiceTopic, $"Exit|{ownerId}");
        }

        public async Task IncrementDownloadSuccessAsync(string agentId, int count, long elapsedMilliseconds)
        {
            await _mq.PublishAsync(DotnetSpiderConsts.StatisticsServiceTopic,
                $"DownloadSuccess|{agentId},{count},{elapsedMilliseconds}");
        }

        public async Task IncrementDownloadFailedAsync(string agentId, int count)
        {
            await _mq.PublishAsync(DotnetSpiderConsts.StatisticsServiceTopic,
                $"DownloadFailed|{agentId},{count}");
        }

        public async Task PrintStatisticsAsync(string ownerId)
        {
            await _mq.PublishAsync(DotnetSpiderConsts.StatisticsServiceTopic,
                $"Print|{ownerId}");
        }

        public async Task IncrementTotalAsync(string ownerId, int count)
        {
            await _mq.PublishAsync(DotnetSpiderConsts.StatisticsServiceTopic,
                $"Total|{ownerId},{count}");
        }
    }
}