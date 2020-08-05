using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace TwitterReader
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
              Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ReadFromTwitterService>();

                    services.AddHttpClient("twitter", (sp,c) =>
                    {
                        var settings = sp.GetService<IOptions<TwitterSettings>>().Value;

                        c.BaseAddress = new Uri(settings.SearchUrl);
                        c.DefaultRequestHeaders.Add("Accept", "application/json");
                        c.DefaultRequestHeaders.Add("authorization", $"bearer {settings.BearerToken}");

                    });

                    services.Configure<TwitterSettings>(options => hostContext.Configuration.GetSection(nameof(TwitterSettings)).Bind(options));

                    services.AddScoped<ITwitterSearch, TwitterSearch>();
                });
    }
}
