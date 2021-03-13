using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using UkrgoParser.Shared.Models.Entities;

namespace UkrgoParser.Client.HttpClients
{
    public class ContactHttpClient
    {
        private readonly HttpClient _httpClient;

        public ContactHttpClient(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IList<Contact>> GetContactsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<Contact>>("");
        }

        public async Task AddContactAsync(Contact contact)
        {
            await _httpClient.PostAsJsonAsync("", contact);
        }
    }
}
