using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterReader
{
    public class TwitterSearch : ITwitterSearch
    {
        private readonly TwitterSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        public TwitterSearch(IOptions<TwitterSettings> settings, IHttpClientFactory httpClientFactory)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                DateFormatString = "ddd MMM dd HH:mm:ss +ffff yyyy"
            };
        }


        public async IAsyncEnumerable<TwitterStatus[]> Search(string searchTerm, DateTime earliestDate)
        {
            DateTime now = DateTime.UtcNow;
            await foreach(var statues in Search(searchTerm, earliestDate, now))
            {
                yield return statues;
            }
        }

        public async IAsyncEnumerable<TwitterStatus[]> Search(string searchTerm, DateTime earliestDate, DateTime latestDate)
        {
            var client = _httpClientFactory.CreateClient("twitter");

            int maxQueries = 450;
            long? lowestId = null;
            while (maxQueries-- > 0)
            {
                string query = $"?q={HttpUtility.UrlEncode(searchTerm)}&result_type=recent&lang=en&count=100&until={latestDate:yyyy-MM-dd}";
                if (lowestId != null)
                {
                    query += $"&max_id={lowestId - 1}";
                }

                var result = await client.GetAsync(query);

                if (!result.IsSuccessStatusCode)
                {
                    await Task.Delay(TimeSpan.FromMinutes(15));
                    result = await client.GetAsync(query);
                }

                var text = await result.Content.ReadAsStringAsync();

                var response = JsonConvert.DeserializeObject<TwitterResponse>(text, _jsonSerializerSettings);

                yield return response.Statuses.Where(s => s.Created_At > earliestDate).ToArray();

                DateTime minDate = response.Statuses.Min(s => s.Created_At);

                if (minDate < earliestDate)
                {
                    break;
                }
                lowestId = response.Statuses.Min(s => s.Id);
            }
        }

       
    }
}
