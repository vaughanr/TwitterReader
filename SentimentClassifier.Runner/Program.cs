using System;
using System.Threading.Tasks;

namespace SentimentAnalyzer.Runner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SentimentClassifierBuilder analyser = new SentimentClassifierBuilder();

            var engine = await analyser.BuildEngine();

            var result = engine.Predict("That was great");
            Console.WriteLine($"{result}");

            Console.Read();
        }
    }
}
