using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System.Collections.Generic;
using System.Text;

namespace SentimentClassifier.Model
{
    public class SentimentData
    {
        [LoadColumn(1), ColumnName("Label")]
        public bool Sentiment { get; set; }

        [LoadColumn(2)]
        public string SentimentText { get; set; }
    }

}
