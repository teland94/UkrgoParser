using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using BlazorPro.BlazorSize;
using BlazorStrap;
using CurrieTechnologies.Razor.Clipboard;
using MatBlazor;
using UkrgoParser.Client.HttpClients;

namespace UkrgoParser.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            var services = builder.Services;

            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            services.AddHttpClient<BrowserHttpClient>(client =>
                client.BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}api/browser/"));
            services.AddHttpClient<BlacklistHttpClient>(client =>
                client.BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}api/blacklist/"));
            services.AddHttpClient<ContactHttpClient>(client =>
                client.BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}api/contact/"));

            services.AddMatBlazor();
            services.AddBootstrapCss();
            services.AddMatToaster(config =>
            {
                config.Position = MatToastPosition.BottomCenter;
                config.ShowCloseButton = false;
                config.ShowProgressBar = false;
                config.MaximumOpacity = 100;
            });
            services.AddBlazoredLocalStorage();
            services.AddClipboard();
            services.AddMediaQueryService();

            await builder.Build().RunAsync();
        }
    }
}
