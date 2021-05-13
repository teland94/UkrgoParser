using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using UkrgoParser.Server.Configuration;
using UkrgoParser.Server.Interfaces;
using UkrgoParser.Server.Models;
using UkrgoParser.Shared.Models.Entities;

namespace UkrgoParser.Server.Services
{
    public class OlxBrowserService : BrowserBaseService, IBrowserService
    {
        public static string AccessToken { get; set; }

        private readonly Regex _phoneReplaceRegex = new(@"[\s\-\(\)]", RegexOptions.Compiled);

        private OlxApiSettings OlxApiSettings { get; }

        public OlxBrowserService(HttpClient client,
            IOptions<OlxApiSettings> olxApiSettingsAccessor) : base(client)
        {
            client.BaseAddress = new Uri("https://www.olx.ua");
            OlxApiSettings = olxApiSettingsAccessor.Value;
        }

        public async Task<IEnumerable<PostLink>> GetPostLinksAsync(Uri uri)
        {
            await LoadPageAsync(uri);

            var postElements = Doc.DocumentNode
                .SelectNodes("//table[contains(@class, 'offers')]//table");

            var postElementsList = new List<PostLink>();
            foreach (var postElement in postElements)
            {
                var link = postElement
                    .SelectSingleNode(".//td[contains(@class, 'title-cell')]//a[contains(@class, 'detailsLink')]");

                var postLink = new PostLink
                {
                    Caption = link?.InnerText.Trim(),
                    ImageUri = new Uri(postElement.SelectSingleNode(".//img").Attributes["src"].Value),
                    Uri = new Uri(link.Attributes["href"].Value),
                    Price = postElement.SelectSingleNode(".//p[contains(@class, 'price')]")?.InnerText.Trim(),
                };

                postElementsList.Add(postLink);
            }

            return postElementsList;
        }

        public async Task<string> GetPhoneNumberAsync(Uri postLinkUri)
        {
            await LoadPageAsync(postLinkUri);

            if (string.IsNullOrEmpty(AccessToken))
            {
                AccessToken = await GetAccessToken();
            }

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var offerElement = Doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'css-7oa68k-Text')]");
            var offerId = Regex.Match(offerElement.InnerText, @"\d+").Value;

            var phoneNumbers = await GetPhoneNumbersAsync(offerId);

            return phoneNumbers.FirstOrDefault();
        }

        public async Task<Post> GetPostDetails(Uri postLinkUri)
        {
            await LoadPageAsync(postLinkUri);

            var header = Doc.DocumentNode.SelectSingleNode("//h1[@data-cy='ad_title']");
            var attributesElements = Doc.DocumentNode.SelectNodes("//ul[contains(@class, 'css-sfcl1s')]/li");
            var descriptionElem = Doc.DocumentNode.SelectSingleNode(".//div[@data-cy='ad_description']/div");
            var imageElements = Doc.DocumentNode.SelectNodes("//div[@data-cy='adPhotos-swiperSlide']//img");
            var priceElem = Doc.DocumentNode.SelectSingleNode("//div[@data-testid='ad-price-container']/h3");

            return new Post
            {
                Title = header.InnerText.Trim(),
                Attributes = attributesElements.Select(elem => elem.InnerText),
                Description = descriptionElem.InnerText.Trim(),
                ImageUris = imageElements.Select(elem => new Uri(elem.Attributes["data-src"]?.Value ?? elem.Attributes["src"].Value)).ToList(),
                Price = priceElem?.InnerText.Trim()
            };
        }

        private async Task<IEnumerable<string>> GetPhoneNumbersAsync(string offerId)
        {
            var response = await HttpClient.GetFromJsonAsync<ApiResponse<PhoneResponse>>($"/api/v1/offers/{offerId}/phones");

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response),"Invalid response");
            }

            return response.Data.Phones.Select(n => _phoneReplaceRegex.Replace(n, ""));
        }

        private async Task<string> GetAccessToken()
        {
            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "scope", "read write v2" },
                { "client_id", OlxApiSettings.ClientId },
                { "client_secret", OlxApiSettings.ClientSecret }
            };

            var result = await SendPostFormUrlEncodedRequestAsync(new Uri("/api/open/oauth/token", UriKind.Relative), parameters);
            var token = JsonSerializer.Deserialize<TokenResponse>(result);

            if (token == null)
            {
                throw new ArgumentNullException(nameof(token), "Token is required");
            }

            return token.AccessToken;
        }
    }
}
