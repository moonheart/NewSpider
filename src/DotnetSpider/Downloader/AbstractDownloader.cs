using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader
{
    public abstract class AbstractDownloader : IDownloader
    {
        public ILogger Logger { get; set; }

        public string AgentId { get; set; }

        protected abstract Task<Response> ImplDownloadAsync(Request request);

        public async Task<Response> DownloadAsync(Request request)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var response = await ImplDownloadAsync(request);
            stopwatch.Stop();
            response.AgentId = AgentId;
            response.Request.AgentId = AgentId;
            response.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            return response;
        }
    }
}