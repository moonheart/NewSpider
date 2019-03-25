using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Data;
using DotnetSpider.Downloader;
using DotnetSpider.Downloader.Entity;
using DotnetSpider.MessageQueue;
using DotnetSpider.RequestSupply;
using DotnetSpider.Scheduler;
using DotnetSpider.Statistics;
using Microsoft.Extensions.Logging;

namespace DotnetSpider
{
    public partial class Spider
    {
        private readonly IList<Request> _requests = new List<Request>();

        private readonly List<IDataFlow> _dataFlows = new List<IDataFlow>();
        private readonly IMessageQueue _mq;
        private readonly ILogger _logger;
        private readonly IDownloadService _downloadService;
        private readonly IStatisticsService _statisticsService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly List<IRequestSupply> _requestSupplies = new List<IRequestSupply>();

        private DateTime _lastRequestedTime;
        private Status _status;
        private IScheduler _scheduler;
        private int _emptySleepTime = 30;
        private int _retryDownloadTimes = 5;
        private int _statisticsInterval = 5;
        private double _speed;
        private int _speedControllerInterval = 1000;
        private int _dequeueBatchCount = 1;
        private int _depth = int.MaxValue;
        private string _id;
        private bool _retryWhenResultIsEmpty;
        private bool _mmfSignal;

        public event Action<Request> OnDownloading;

        public DownloaderOptions DownloaderOptions { get; set; } = new DownloaderOptions();

        /// <summary>
        /// 遍历深度
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public int Depth
        {
            get => _depth;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("遍历深度必须大于 0");
                }

                CheckIfRunning();
                _depth = value;
            }
        }

        /// <summary>
        /// 是否支持通过 MMF 操作爬虫
        /// </summary>
        public bool MmfSignal
        {
            get => _mmfSignal;
            set
            {
                CheckIfRunning();
                _mmfSignal = value;
            }
        }

        /// <summary>
        /// 如果结析结果为空, 重试。默认值为 否。
        /// </summary>
        public bool RetryWhenResultIsEmpty
        {
            get => _retryWhenResultIsEmpty;
            set
            {
                CheckIfRunning();
                _retryWhenResultIsEmpty = value;
            }
        }

        /// <summary>
        /// 爬虫运行状态
        /// </summary>
        public Status Status => _status;

        public IScheduler Scheduler
        {
            get => _scheduler;
            set
            {
                CheckIfRunning();
                if (_scheduler.Total > 0)
                {
                    throw new SpiderException("当调度器不为空时，不能更换调度器");
                }

                _scheduler = value;
            }
        }

        /// <summary>
        /// 每秒尝试下载多少个请求
        /// </summary>
        public double Speed
        {
            get => _speed;
            set
            {
                if (value <= 0)
                {
                    throw new SpiderException("下载速度必须大于 0");
                }

                CheckIfRunning();

                _speed = value;

                if (_speed >= 1)
                {
                    _speedControllerInterval = 1000;
                    _dequeueBatchCount = (int) _speed;
                }
                else
                {
                    _speedControllerInterval = (int) (1 / _speed) * 1000;
                    _dequeueBatchCount = 1;
                }

                var maybeEmptySleepTime = _speedControllerInterval / 1000;
                if (maybeEmptySleepTime >= EmptySleepTime)
                {
                    var larger = (int) (maybeEmptySleepTime * 1.5);
                    EmptySleepTime = larger > 30 ? larger : 30;
                }
            }
        }

        public int EnqueueBatchCount { get; set; } = 1000;

        /// <summary>
        /// 上报状态的间隔时间，单位: 秒
        /// </summary>
        /// <exception cref="SpiderException"></exception>
        public int StatisticsInterval
        {
            get => _statisticsInterval;
            set
            {
                if (value < 5)
                {
                    throw new SpiderException("上报状态间隔必须大于 5 (秒)");
                }

                CheckIfRunning();
                _statisticsInterval = value;
            }
        }

        /// <summary>
        /// 任务的唯一标识
        /// </summary>
        public string Id
        {
            get => _id;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("任务标识不能为空");
                }

                if (value.Length > 100)
                {
                    throw new ArgumentException("任务标识长度不能超过 100");
                }

                CheckIfRunning();

                _id = value;
            }
        }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 下载重试次数
        /// </summary>
        /// <exception cref="SpiderException"></exception>
        public int RetryDownloadTimes
        {
            get => _retryDownloadTimes;
            set
            {
                if (value <= 0)
                {
                    throw new SpiderException("下载重试次数必须大于 0");
                }

                CheckIfRunning();
                _retryDownloadTimes = value;
            }
        }

        public int EmptySleepTime
        {
            get => _emptySleepTime;
            set
            {
                if (value <= _speedControllerInterval)
                {
                    throw new SpiderException($"等待结束时间必需大于速度控制器间隔: {_speedControllerInterval}");
                }

                if (value < 0)
                {
                    throw new SpiderException("等待结束时间必需大于 0 (秒)");
                }

                CheckIfRunning();
                _emptySleepTime = value;
            }
        }
    }
}