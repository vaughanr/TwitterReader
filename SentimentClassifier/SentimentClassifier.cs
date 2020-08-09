using Microsoft.ML;
using SentimentClassifier.Model;

namespace SentimentAnalyzer
{
    public class SentimentClassifier
    {
        private readonly PredictionEngine<SentimentData, SentimentPrediction> predictionEngine;

        public SentimentClassifier(PredictionEngine<SentimentData, SentimentPrediction> predictionEngine)
        {
            this.predictionEngine = predictionEngine;
        }

        public (Sentiment sentiment, float probability, float score) Predict(string text)
        {
            var result = predictionEngine.Predict(new SentimentData { SentimentText = text });

            return (result.Prediction ? Sentiment.Good : Sentiment.Bad, result.Probability, result.Score);
        }
    }
}
