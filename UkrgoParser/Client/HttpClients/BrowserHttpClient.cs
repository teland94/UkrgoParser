using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using UkrgoParser.Shared.Models.Entities;

namespace UkrgoParser.Client.HttpClients
{
    public class BrowserHttpClient
    {
        private readonly HttpClient _httpClient;

        public BrowserHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<PostLink>> GetPostLinksAsync(Uri uri)
        {
            return await _httpClient.GetFromJsonAsync<IList<PostLink>>($"GetPostLinks?uri={uri}");
        }

        public async Task<string> GetPhoneNumberAsync(Uri postLinkUri)
        {
            return await _httpClient.GetStringAsync($"GetPhoneNumber?postLinkUri={postLinkUri}");
        }

        public async Task<Post> GetPostDetailsAsync(Uri postLinkUri)
        {
            return await _httpClient.GetFromJsonAsync<Post>($"GetPostDetails?postLinkUri={postLinkUri}");
        }

        public async Task<byte[]> GetImageAsync(Uri imageUri, bool cropUnwantedBackground = false)
        {
            return await _httpClient.GetByteArrayAsync(
                $"GetImage?imageUri={imageUri}&cropUnwantedBackground={cropUnwantedBackground}");
        }
    }
}
