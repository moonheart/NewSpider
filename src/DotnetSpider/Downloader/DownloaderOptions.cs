using System;
using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core;

namespace DotnetSpider.Downloader
{
    public class DownloaderOptions
    {
        /// <summary>
        /// Cookies
        /// </summary>
        private readonly HashSet<Cookie> _cookies = new HashSet<Cookie>();

        public DownloaderType Type { get; set; } = DownloaderType.HttpClient;

        public Cookie[] Cookies => _cookies.ToArray();

        public bool UseProxy { get; set; } = false;

        public bool UseCookies { get; set; } = true;

        public bool AllowAutoRedirect { get; set; } = true;

        public int Timeout { get; set; } = 5000;

        public bool DecodeHtml { get; set; }

        public int DownloaderCount { get; set; }

        /// <summary>
        /// Add one cookie to downloader
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 添加Cookie
        /// </summary>
        /// <param name="name">名称(<see cref="Cookie.Name"/>)</param>
        /// <param name="value">值(<see cref="Cookie.Value"/>)</param>
        /// <param name="domain">作用域(<see cref="Cookie.Domain"/>)</param>
        /// <param name="path">作用路径(<see cref="Cookie.Path"/>)</param>
        public void AddCookie(string name, string value, string domain, string path = "/")
        {
            var cookie = new Cookie(name, value, domain, path);
            AddCookie(cookie);
        }

        /// <summary>
        /// Add cookies to downloader
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 添加Cookies
        /// </summary>
        /// <param name="cookies">Cookies的键值对 (Cookie's key-value pairs)</param>
        /// <param name="domain">作用域(<see cref="Cookie.Domain"/>)</param>
        /// <param name="path">作用路径(<see cref="Cookie.Path"/>)</param>
        public void AddCookies(IDictionary<string, string> cookies, string domain, string path = "/")
        {
            foreach (var pair in cookies)
            {
                var name = pair.Key;
                var value = pair.Value;
                AddCookie(name, value, domain, path);
            }
        }

        /// <summary>
        /// Add cookies to downloader
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 设置 Cookies
        /// </summary>
        /// <param name="cookies">Cookies的键值对字符串, 如: a1=b;a2=c;(Cookie's key-value pairs string, a1=b;a2=c; etc.)</param>
        /// <param name="domain">作用域(<see cref="Cookie.Domain"/>)</param>
        /// <param name="path">作用路径(<see cref="Cookie.Path"/>)</param>
        public void AddCookies(string cookies, string domain, string path = "/")
        {
            var pairs = cookies.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                var name = keyValue[0];
                string value = keyValue.Length > 1 ? keyValue[1] : string.Empty;
                AddCookie(name, value, domain, path);
            }
        }

        /// <summary>
        /// Add one cookie to downloader
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 设置 Cookie
        /// </summary>
        /// <param name="cookie">Cookie</param>
        public virtual void AddCookie(Cookie cookie)
        {
            _cookies.Add(cookie);
        }
    }
}