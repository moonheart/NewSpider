using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Core
{
    /// <summary>
    /// 启动任务工具
    /// </summary>
    public static class Startup
    {
        private static readonly Dictionary<string, string> SwitchMappings =
            new Dictionary<string, string>
            {
                {"-s", "spider"},
                {"-n", "name"},
                {"-i", "id"},
                {"-c", "config"},
                {"-a", "args"},
            };

        /// <summary>
        /// DLL 名字中包含任意一个即是需要扫描的 DLL
        /// </summary>
        public static readonly List<string> DetectNames = new List<string> {"dotnetspider.sample"};

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="args">运行参数</param>
        public static void Run(params string[] args)
        {
            Framework.SetEncoding();

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddCommandLine(args, SwitchMappings);
            configurationBuilder.AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();
            string spider = configuration["spider"];
            if (string.IsNullOrWhiteSpace(spider))
            {
                throw new SpiderException("未指定需要执行的爬虫");
            }

            string name = configuration["name"];
            string id = configuration["id"] ?? Guid.NewGuid().ToString("N");
            string config = configuration["config"];
            var arguments = configuration["args"]?.Split(' ');

            PrintEnvironment(args);

            // TODO: 根据配置文件启用不同的 Builder
            var builder = new LocalSpiderBuilder();
            builder.UseSerilog(); // 可以配置任意日志组件
            builder.UseConfiguration(config);

            var spiderTypes = DetectSpiders();

            if (spiderTypes == null || spiderTypes.Count == 0)
            {
                return;
            }

            var spiderType = spiderTypes.FirstOrDefault(x => x.Name.ToLower() == spider.ToLower());
            if (spiderType == null)
            {
                ConsoleHelper.WriteLine($"未找到爬虫: {spider}", 0, ConsoleColor.DarkYellow);
                return;
            }

            builder.Services.AddTransient(spiderType);
            var instance = builder.Build(spiderType);
            if (instance != null)
            {
                instance.Name = name;
                instance.Id = id;
                instance.RunAsync(arguments);
            }
            else
            {
                ConsoleHelper.WriteLine("创建爬虫对象失败", 0, ConsoleColor.DarkYellow);
            }
        }

        /// <summary>
        /// 检测爬虫类型
        /// </summary>
        /// <returns></returns>
        private static HashSet<Type> DetectSpiders()
        {
            var spiderTypes = new HashSet<Type>();

            var spiderType = typeof(Spider);
            var asmNames = new List<string>();
            foreach (var file in DetectAssemblies())
            {
                var asm = Assembly.Load(file);
                var types = asm.GetTypes();
                asmNames.Add(asm.GetName(false).Name);

                foreach (var type in types)
                {
                    if (spiderType.IsAssignableFrom(type))
                    {
                        spiderTypes.Add(type);
                    }
                }
            }

            ConsoleHelper.WriteLine($"程序集     : {string.Join(", ", asmNames)}", 0, ConsoleColor.DarkYellow);
            ConsoleHelper.WriteLine($"检测到爬虫 : {spiderTypes.Count} 个", 0, ConsoleColor.DarkYellow);

            return spiderTypes;
        }

        /// <summary>
        /// 扫描所有需要求的DLL
        /// </summary>
        /// <returns></returns>
        private static List<string> DetectAssemblies()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
            var files = Directory.GetFiles(path)
                .Where(f => f.EndsWith(".dll") || f.EndsWith(".exe"))
                .Select(f => Path.GetFileName(f).Replace(".dll", "").Replace(".exe", "")).ToList();
            return
                files.Where(f => !f.EndsWith("DotnetSpider")
                                 && DetectNames.Any(n => f.ToLower().Contains(n))).ToList();
        }

        private static void PrintEnvironment(params string[] args)
        {
            Framework.PrintInfo();
            var commands = string.Join(" ", args);
            ConsoleHelper.WriteLine($"运行参数   : {commands}", 0, ConsoleColor.DarkYellow);
            ConsoleHelper.WriteLine($"运行目录   : {AppDomain.CurrentDomain.BaseDirectory}", 0,
                ConsoleColor.DarkYellow);
            ConsoleHelper.WriteLine(
                $"操作系统   : {Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "X64" : "X86")}", 0,
                ConsoleColor.DarkYellow);
        }
    }
}