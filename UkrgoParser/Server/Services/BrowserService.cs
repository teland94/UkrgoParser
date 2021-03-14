using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using UkrgoParser.Server.Helpers;
using UkrgoParser.Shared.Models.Entities;

namespace UkrgoParser.Server.Services
{
    public interface IBrowserService
    {
        Task<IEnumerable<PostLink>> GetPostLinksAsync(Uri uri);

        Task<string> GetPhoneNumberAsync(Uri postLinkUri);

        Task<Post> GetPostDetails(Uri postLinkUri);

        Task<byte[]> GetImage(Uri imageUri, bool cropUnwantedBackground = false);
    }

    public class BrowserService : IBrowserService
    {
        private readonly HtmlDocument _doc;
        private readonly HttpClient _httpClient;

        public BrowserService(HttpClient client)
        {
            _doc = new HtmlDocument { OptionReadEncoding = false };
            _httpClient = client;
        }

        public async Task<IEnumerable<PostLink>> GetPostLinksAsync(Uri uri)
        {
            await LoadPageAsync(uri);

            return _doc.DocumentNode
                .SelectNodes("//div[contains(@class, 'post_top')]/div[contains(@class, 'post')]//a[contains(@class, 'link_post')]|//div[contains(@class, 'main-content')]//table//td/h3/a[contains(@class, 'link_post')]")
                .Where(elem => elem.Attributes["href"].Value.StartsWith("http"))
                .Select(elem => new PostLink
                {
                    Caption = elem.InnerText.Trim(),
                    Uri = new Uri(elem.Attributes["href"].Value)
                }).ToList();
        }

        public async Task<string> GetPhoneNumberAsync(Uri postLinkUri)
        {
            await LoadPageAsync(postLinkUri);

            var postPhonesShowDiv = _doc.DocumentNode.SelectSingleNode("//div[@id='post-phones-show-div']");

            if (postPhonesShowDiv == null) return null;

            var input = postPhonesShowDiv.SelectSingleNode(".//input");
            var onCLickStr = input?.GetAttributeValue("onclick", null);

            var funcArgs = GetFuncArgs(onCLickStr).ToArray();
            var formData = new Dictionary<string, string> { ["i"] = funcArgs[0], ["s"] = funcArgs[1] };
            var postContactsDiv = await SendPostRequestAsync(new Uri("http://ukrgo.com/moduls/showphonesnumbers.php"), formData);

            if (!string.IsNullOrEmpty(postContactsDiv))
            {
                _doc.LoadHtml(postContactsDiv);
                var postContactsSpan = _doc.DocumentNode.SelectSingleNode("//span");

                return postContactsSpan?.InnerText;
            }

            return null;
        }

        public async Task<Post> GetPostDetails(Uri postLinkUri)
        {
            await LoadPageAsync(postLinkUri);

            var detailsTable = _doc.DocumentNode.SelectSingleNode("//table[4]//table");
            var header = detailsTable.SelectSingleNode(".//tr[1]/td/h1");
            var attributesDiv = detailsTable.SelectSingleNode(".//tr[2]/td/div");
            var descriptionDiv = detailsTable.SelectSingleNode(".//tr[3]/td/div");
            var imageUris = detailsTable
                .SelectNodes(".//tr[5]/td/table//tr//img")
                ?.Select(elem => new Uri(postLinkUri.GetLeftPart(UriPartial.Authority) + elem.Attributes["src"].Value[1..]))
                .Distinct()
                .ToList();

            return new Post
            {
                Title = header.InnerText.Trim(),
                Attributes = attributesDiv.InnerHtml.Trim(),
                Description = descriptionDiv.ChildNodes[0].InnerText.Trim(),
                ImageUris = imageUris
            };
        }

        public async Task<byte[]> GetImage(Uri imageUri, bool cropUnwantedBackground = false)
        {
            using var http = new HttpClient();
            var imageData = await http.GetByteArrayAsync(imageUri);
            return cropUnwantedBackground ? await ImageHelper.CropUnwantedBackground(imageData) : imageData;
        }

        private async Task<string> SendPostRequestAsync(Uri uri, IDictionary<string, string> data)
        {
            using var http = new HttpClient();
            var content = new FormUrlEncodedContent(data);

            var res = await http.PostAsync(uri, content);
            return await res.Content.ReadAsStringAsync();
        }

        private IEnumerable<string> GetFuncArgs(string func)
        {
            var extractFuncRegex = @"\b[^()]+\((.*)\)";
            var extractArgsRegex = @"([^,]+\(.+?\))|([^,]+)";

            var match = Regex.Match(func, extractFuncRegex);
            var innerArgs = match.Groups[1].Value;
            var matches = Regex.Matches(innerArgs, extractArgsRegex);

            return matches.OfType<Match>().Select(m => m.Value.Replace("'", "").Replace(" ", ""));
        }

        private async Task LoadPageAsync(Uri uri)
        {
            var response = await _httpClient.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            var source = await response.Content.ReadAsStringAsync();
            source = WebUtility.HtmlDecode(source);

            _doc.LoadHtml(source);
        }
    }
}
