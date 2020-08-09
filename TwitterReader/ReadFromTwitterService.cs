using Microsoft.Extensions.Hosting;
using SentimentAnalyzer;
using SentimentClassifier.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Console.WriteLine($"Building sentiment classifier...");

            var sw = new Stopwatch();
            sw.Start();
            SentimentClassifierBuilder analyser = new SentimentClassifierBuilder();

            var engine = await analyser.BuildEngine();
            sw.Stop();
            Console.WriteLine($"Finished in {(int)sw.Elapsed.TotalSeconds}s");

            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"Please enter search term.");

                string line = Console.ReadLine();

                int bad = 0;
                int good = 0;

                int maxSamples = 3;
                List<string> badSamples = new List<string>();
                List<string> goodSamples = new List<string>();

                await foreach (var statuses in _twitterSearch.Search(line, DateTime.UtcNow.AddDays(-1)))
                {
                    foreach(var status in statuses)
                    {
                        var result = engine.Predict( status.Text );

                        if (result.sentiment == Sentiment.Good)
                        {
                            good++;
                            if(goodSamples.Count < maxSamples)
                            {
                                goodSamples.Add($"Probability:{result.probability}{Environment.NewLine}Score:{result.score}{Environment.NewLine}{status.Text}{Environment.NewLine}");
                            }
                        }
                        else
                        {
                            bad++;
                            if (badSamples.Count < maxSamples)
                            {
                                badSamples.Add($"Probability:{result.probability}{Environment.NewLine}Score:{result.score}{Environment.NewLine}{status.Text}{Environment.NewLine}");
                            }
                        }
                    }
                }

                Console.WriteLine("****** Sample bad tweets ******");
                foreach(string tweet in badSamples)
                {
                    Console.WriteLine(tweet);
                }


                Console.WriteLine("****** Sample good tweets ******");
                foreach (string tweet in goodSamples)
                {
                    Console.WriteLine(tweet);
                }

                Console.WriteLine($"Total: {bad} bad and {good} good");
            }
        }
    }
}
