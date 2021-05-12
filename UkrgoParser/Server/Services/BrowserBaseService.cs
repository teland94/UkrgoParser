using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using UkrgoParser.Server.Helpers;

namespace UkrgoParser.Server.Services
{
    public class BrowserBaseService
    {
        protected readonly HtmlDocument Doc;
        protected readonly HttpClient HttpClient;

        public BrowserBaseService(HttpClient client)
        {
            Doc = new HtmlDocument { OptionReadEncoding = false };
            HttpClient = client;
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

        protected async Task LoadPageAsync(Uri uri)
        {
            var response = await HttpClient.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            var source = await response.Content.ReadAsStringAsync();
            source = WebUtility.HtmlDecode(source);

            Doc.LoadHtml(source);
        }

        public async Task<byte[]> GetImage(Uri imageUri, bool cropUnwantedBackground = false)
        {
            var imageData = await HttpClient.GetByteArrayAsync(imageUri);
            return cropUnwantedBackground ? await ImageHelper.CropUnwantedBackground(imageData) : imageData;
        }
    }
}
