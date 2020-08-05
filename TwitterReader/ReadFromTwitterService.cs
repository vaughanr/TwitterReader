using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TwitterReader
{
    public class ReadFromTwitterService : BackgroundService
    {
        private readonly ITwitterSearch _twitterSearch;

        public ReadFromTwitterService(ITwitterSearch twitterSearch)
        {
            this._twitterSearch = twitterSearch;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"Please enter search term.");

                string line = Console.ReadLine();

                await foreach(var statuses in _twitterSearch.Search(line, DateTime.UtcNow.AddMinutes(-300)))
                {
                    Console.WriteLine($"{statuses.Length} statuses received");
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
            }

        }
    }
}
