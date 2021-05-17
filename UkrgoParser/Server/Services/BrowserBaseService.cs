using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using UkrgoParser.Server.Helpers;

namespace UkrgoParser.Server.Services
{
    public class BrowserBaseService
    {
        protected readonly HtmlDocument Doc;
        protected readonly HttpClient HttpClient;
        protected readonly IMemoryCache MemoryCache;

        public BrowserBaseService(HttpClient client,
            IMemoryCache memoryCache)
        {
            Doc = new HtmlDocument { OptionReadEncoding = false };
            HttpClient = client;
            MemoryCache = memoryCache;
        }

        public async Task<byte[]> GetImage(Uri imageUri, bool cropUnwantedBackground = false)
        {
            var imageData = await HttpClient.GetByteArrayAsync(imageUri);
            return cropUnwantedBackground ? await ImageHelper.CropUnwantedBackground(imageData) : imageData;
        }

        protected async Task<string> SendPostFormUrlEncodedRequestAsync(Uri uri, IDictionary<string, string> data)
        {
            var content = new FormUrlEncodedContent(data);

            var res = await HttpClient.PostAsync(uri, content);
            return await res.Content.ReadAsStringAsync();
        }

        protected IEnumerable<string> GetFuncArgs(string func)
        {
            var extractFuncRegex = @"\b[^()]+\((.*)\)";
            var extractArgsRegex = @"([^,]+\(.+?\))|([^,]+)";

            var match = Regex.Match(func, extractFuncRegex);
            var innerArgs = match.Groups[1].Value;
            var matches = Regex.Matches(innerArgs, extractArgsRegex);

            return matches.OfType<Match>().Select(m => m.Value.Replace("'", "").Replace(" ", ""));
        }

        protected void RemoveCachedUris(IEnumerable<Uri> uris)
        {
            foreach (var uri in uris)
            {
                MemoryCache.Remove(uri);
            }
        }

        protected async Task LoadPageAsync(Uri uri, bool useCache = false)
        {
            string result;

            if (useCache)
            {
                if (!MemoryCache.TryGetValue(uri, out result))
                {
                    var source = await GetPageAsync(uri);
                    result = MemoryCache.Set(uri, source, DateTimeOffset.Now.AddHours(1));
                }
            }
            else
            {
                result = await GetPageAsync(uri);
            }

            Doc.LoadHtml(result);
        }

        private async Task<string> GetPageAsync(Uri uri)
        {
            var response = await HttpClient.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            var source = await response.Content.ReadAsStringAsync();
            source = WebUtility.HtmlDecode(source);

            return source;
        }
    }
}
