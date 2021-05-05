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

            var postElements = _doc.DocumentNode
                .SelectNodes("//div[contains(@class, 'post_top')]/div[contains(@class, 'post')]|//div[contains(@class, 'main-content')]//table");

            return (from postElement in postElements 
                let postLinkElem = postElement.SelectSingleNode(".//a[contains(@class, 'link_post') and not(img)]") 
                where postLinkElem.Attributes["href"].Value.StartsWith("http") 
                let postImgElem = postElement.SelectSingleNode(".//img")
                let postLinkUri = new Uri(postLinkElem.Attributes["href"].Value)
                select new PostLink
                {
                    ImageUri = new Uri(postLinkUri.GetLeftPart(UriPartial.Authority) 
                                       + postImgElem.Attributes["src"].Value[postImgElem.Attributes["src"].Value.IndexOf("/", StringComparison.OrdinalIgnoreCase)..]),
                    Caption = postLinkElem.InnerText.Trim(), 
                    Uri = postLinkUri
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
                ?.Select(elem => new Uri(postLinkUri.GetLeftPart(UriPartial.Authority) 
                                         + elem.Attributes["src"].Value[elem.Attributes["src"].Value.IndexOf("/", StringComparison.OrdinalIgnoreCase)..]))
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
            var imageData = await _httpClient.GetByteArrayAsync(imageUri);
            return cropUnwantedBackground ? await ImageHelper.CropUnwantedBackground(imageData) : imageData;
        }

        private async Task<string> SendPostRequestAsync(Uri uri, IDictionary<string, string> data)
        {
            var content = new FormUrlEncodedContent(data);

            var res = await _httpClient.PostAsync(uri, content);
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
