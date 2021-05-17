using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using UkrgoParser.Server.Configuration;
using UkrgoParser.Server.Filters;
using UkrgoParser.Server.Handlers;
using UkrgoParser.Server.Interfaces;
using UkrgoParser.Server.Services;

namespace UkrgoParser.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddSwaggerGen();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.Configure<ProxySettings>(Configuration.GetSection("Proxy"));
            services.Configure<OlxApiSettings>(Configuration.GetSection("OlxApi"));

            services.AddTransient<BrowserHttpClientHandler>();

            services.AddHttpClient<IBrowserService, OlxBrowserService>()
                .ConfigurePrimaryHttpMessageHandler<BrowserHttpClientHandler>();

            services.AddTransient<IBlacklistService, BlacklistService>();
            services.AddTransient<IContactService, ContactService>();

            services.AddScoped<DelayFilter>();

            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "UkrgoParser API");
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
