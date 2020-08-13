using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TwitterReader;

namespace TweetMiner
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient("twitter", (sp, c) =>
                    {
                        var settings = sp.GetService<IOptions<TwitterSettings>>().Value;

                        c.BaseAddress = new Uri(settings.SearchUrl);
                        c.DefaultRequestHeaders.Add("Accept", "application/json");
                        c.DefaultRequestHeaders.Add("authorization", $"bearer {settings.BearerToken}");

                    });

                    services.Configure<TwitterSettings>(options => hostContext.Configuration.GetSection(nameof(TwitterSettings)).Bind(options));

                    services.AddSingleton<ITwitterSearch, TwitterSearch>();
                    services.AddHostedService<Worker>();
                });
    }
}
