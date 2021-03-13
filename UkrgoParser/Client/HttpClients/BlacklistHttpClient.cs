using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using UkrgoParser.Shared.Models.Request;

namespace UkrgoParser.Client.HttpClients
{
    public class BlacklistHttpClient
    {
        private readonly HttpClient _httpClient;

        public BlacklistHttpClient(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IList<string>> GetPhoneNumbersAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<string>>("");
        }

        public async Task<bool> CheckPhoneNumberAsync(string phoneNumber)
        {
            var validNumberStr = await _httpClient.GetStringAsync($"CheckNumber?phoneNumber={phoneNumber}");
            return Convert.ToBoolean(validNumberStr);
        }

        public async Task<HttpResponseMessage> AddPhoneNumberAsync(string phoneNumber)
        {
            var response = await _httpClient.PostAsJsonAsync("", new BlockNumberRequestModel
            {
                PhoneNumber = phoneNumber
            });
            return response;
        }

        public async Task DeletePhoneNumberAsync(string phoneNumber)
        {
            await _httpClient.DeleteAsync($"?phoneNumber={phoneNumber}");
        }
    }
}
