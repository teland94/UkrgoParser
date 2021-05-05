using System.Net.Http;
using Microsoft.Extensions.Options;
using MihaZupan;
using UkrgoParser.Server.Configuration;

namespace UkrgoParser.Server.Handlers
{
    public class BrowserHttpClientHandler : HttpClientHandler
    {
        public BrowserHttpClientHandler(IOptions<ProxySettings> proxySettingsAccessor)
        {
            var proxySettings = proxySettingsAccessor.Value;
            if (!string.IsNullOrEmpty(proxySettings.Hostname) && proxySettings.Port > 0)
            {
                Proxy = new HttpToSocks5Proxy(proxySettings.Hostname, proxySettings.Port, proxySettings.Username, proxySettings.Password);
            }
        }
    }
}