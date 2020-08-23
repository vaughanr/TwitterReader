using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace StockPriceMiner
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly AlphavantageSettings _settings;
        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, IOptions<AlphavantageSettings> settings)
        {
            _logger = logger;
            this.httpClientFactory = httpClientFactory;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {


            var companies = new (string company, string ticker)[]
            {
                            ("microsoft","msft") ,
                            (    "apple","AAPL" ),
                            (    "google","GOOGL") ,
                            (    "amazon","AMZN") ,
                            (    "facebook",  "FB") ,
                            (    "ibm",       "IBM") ,
                            (    "dell",      "DELL") ,
                            (    "sony",      "SNE") ,
                            (    "panasonic", "PCRFY") ,
                            (    "intel",     "INTC"),
                            (    "hp"    ,     "HP")
            };


            var client = httpClientFactory.CreateClient("alphavantage");
            string baseURl = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&apikey={_settings.ApiKey}";


            foreach (var c in companies)
            {
                var model = await GetModel(client, baseURl, c.ticker);


                if (!string.IsNullOrWhiteSpace(model.Note))
                {
                    await Task.Delay(TimeSpan.FromMinutes(5));
                    model = await GetModel(client, baseURl, c.ticker);

                    if (!string.IsNullOrWhiteSpace(model.Note))
                    {
                        throw new Exception(model.Note);
                    }
                }

                File.WriteAllText($"data/{c.company}.json", JsonConvert.SerializeObject(model, Formatting.Indented));
            }


            _logger.LogInformation("Finsihed");
        }

        private async Task<StockPriceModel> GetModel(HttpClient client, string baseUrl, string ticker)
        {
            var response = await client.GetAsync($"{baseUrl}&symbol={ticker}");
            response.EnsureSuccessStatusCode();


            string content = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<StockPriceModel>(content);
            return model;
        }
    }
}
