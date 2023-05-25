using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNumberScraper
{
    internal class ScraperBase
    {
        public ILogger Logger { get; }

        public ScraperBase(ILogger logger)
        {
            Logger = logger;
        }
        protected HttpClient CreateHttpClient()
        {
            var client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            });
            client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            return client;
        }

        protected async Task<HttpResponseMessage> PostAsyncWithRetry(HttpClient client, string? uri, HttpContent? content, int retryCount = 3)
        {
            return await PostAsyncWithRetryCore(client, uri, content, 0, retryCount);
        }

        private async Task<HttpResponseMessage> PostAsyncWithRetryCore(HttpClient client, string? uri, HttpContent? content, int retryCount, int maxRetries)
        {
            var response = await client.PostAsync(uri, content);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else
            {
                if (retryCount >= maxRetries)
                {
                    return response;
                }
                else
                {
                    return await PostAsyncWithRetryCore(client, uri, content, retryCount++, maxRetries);
                }
            }
        }

        protected async Task<HttpResponseMessage> GetAsyncWithRetry(HttpClient client, string? uri, int retryCount = 3)
        {
            return await GetAsyncWithRetryCore(client, uri, 0, retryCount);
        }

        private async Task<HttpResponseMessage> GetAsyncWithRetryCore(HttpClient client, string? uri, int retryCount, int maxRetries)
        {
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else
            {
                if (retryCount >= maxRetries)
                {
                    return response;
                }
                else
                {
                    return await GetAsyncWithRetryCore(client, uri, retryCount++, maxRetries);
                }
            }
        }

        /// <summary>
        /// Returns true if the condition is True and logs the given message
        /// </summary>
        protected bool LogIfTrue(bool condition, string message)
        {
            if (condition)
            {
                Logger.LogError(message);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the @object is NULL and logs the given message
        /// </summary>
        protected bool LogIfNull(object? @object, string message)
        {
            if (@object == null)
            {
                Logger.LogError(message);
                return true;
            }
            return false;
        }
    }
}
