using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SentimentAnalyzer;
using SentimentClassifier.Model;
using TwitterReader;

namespace TweetMiner
{
    public partial class Worker : BackgroundService
    {

        private readonly ITwitterSearch _twitterSearch;

        public Worker(ITwitterSearch twitterSearch)
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


            string[] searches = new string[]
            {
                "microsoft",
                "apple",
                "google",
                "amazon",
                "facebook",
                "samsung",
                "huawei",
                "ibm",
                "dell",
                "sony",
                "panasonic",
                "intel",
                "hp"
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var line in searches)
                {

                    HashSet<string> allTweets = new HashSet<string>();

                    ConcurrentDictionary<DateTime, DailySentiment> dailySentiments = new ConcurrentDictionary<DateTime, DailySentiment>();

                    var dates = new DateTime[]
                    {
                    DateTime.Today,
                    DateTime.Today.AddDays(-1),
                    DateTime.Today.AddDays(-2),
                    DateTime.Today.AddDays(-3),
                    DateTime.Today.AddDays(-4),
                    DateTime.Today.AddDays(-5),
                    DateTime.Today.AddDays(-6),
                    };

                    foreach (var date in dates)
                    {
                        if (File.Exists(GetFileName(line, date.AddDays(-1))))
                        {
                            continue;
                        }

                        await foreach (var statuses in _twitterSearch.Search(line, date.AddDays(-1), date))
                        {
                            foreach (var status in statuses)
                            {
                                if (!allTweets.Add(status.Text))
                                {
                                    continue;
                                }

                                var result = engine.Predict(status.Text);
                                var dailySentiment = dailySentiments.GetOrAdd(status.Created_At.Date, new DailySentiment { SearchTerm = line, Date = status.Created_At.Date });
                                if (result.sentiment == Sentiment.Good)
                                {
                                    dailySentiment.PositiveCount++;
                                }
                                else
                                {
                                    dailySentiment.NegativeCount++;
                                }
                            }
                        }


                        foreach (var dailySentiment in dailySentiments)
                        {
                            string text = JsonConvert.SerializeObject(dailySentiment);
                            string fileName = GetFileName(line, dailySentiment.Key);
                            File.WriteAllText(fileName, text);
                            Console.WriteLine($"Saved {fileName}");
                        }
                    }

                    string GetFileName(string search, DateTime date)
                    {
                        return $"{search}_{date:yyyyMMdd}.json";
                    }
                }
                
            }
        }
    }
}
