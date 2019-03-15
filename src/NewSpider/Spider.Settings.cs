using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewSpider.Data;
using NewSpider.Data.Pipeline;
using NewSpider.Data.Processor;
using NewSpider.Downloader;

namespace NewSpider
{
    public partial class Spider
    {
        public void AddProcessor(IPageProcessor processor)
        {
            _processors.Add(processor);
        }

        public void AddPipeline(IPipeline pipeline)
        {
            _pipelines.Add(pipeline);
        }

        public void AddDataFlow(IDataFlow dataFlow)
        {
            _dataFlows.Add(dataFlow);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ISpider AddRequests(params Request[] requests)
        {
            foreach (var request in requests)
            {
                request.OwnerId = Id;
                _requests.Add(request);
                if (_requests.Count == RequestBatchCount)
                {
                    var count = _scheduler.PushAsync(Id, _requests).GetAwaiter().GetResult();

                    _statisticsService.TotalAsync(Id, count).ConfigureAwait(false);
                    _logger.LogInformation($"任务 {Id} 请求推送到调度器: {RequestBatchCount}");
                    _requests.Clear();
                }
            }

            return this;
        }
    }
}