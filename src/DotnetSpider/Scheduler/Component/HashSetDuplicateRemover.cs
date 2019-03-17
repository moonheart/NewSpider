using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using DotnetSpider.Data;
using DotnetSpider.Downloader;

namespace DotnetSpider.Scheduler.Component
{
    /// <summary>
    /// 通过哈希去重
    /// </summary>
    public class HashSetDuplicateRemover : IDuplicateRemover
    {
        private readonly ConcurrentDictionary<string, bool> _hashes = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// Check whether the request is duplicate.
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Whether the request is duplicate.</returns>
        public bool IsDuplicate(Request request)
        {
            Check.NotNull(request.OwnerId,nameof(request.OwnerId));
            var hash = request.Hash;
            bool isDuplicate = _hashes.ContainsKey(hash);
            if (!isDuplicate)
            {
                _hashes.TryAdd(hash, true);
            }

            return isDuplicate;
        }

        /// <summary>
        /// Reset duplicate check.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ResetDuplicateCheck()
        {
            _hashes.Clear();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _hashes.Clear();
        }
    }
}