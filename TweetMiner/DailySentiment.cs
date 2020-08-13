using System;

namespace TweetMiner
{
    public partial class Worker
    {
        public class DailySentiment
        {
            public string SearchTerm { get; set; }
            public int PositiveCount { get; set; }
            public int NegativeCount { get; set; }
            public DateTime Date { get; set; }
        }
    }
}
