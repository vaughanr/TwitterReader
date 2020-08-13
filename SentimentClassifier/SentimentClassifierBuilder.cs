using Microsoft.ML;
using Microsoft.ML.Data;
using SentimentClassifier.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using static Microsoft.ML.DataOperationsCatalog;

namespace SentimentAnalyzer
{
    public class SentimentClassifierBuilder
    {
        private readonly string _dataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Data", "Train.csv");
        private readonly MLContext _mlContext;
        private readonly TrainTestData _splitDataView;
        public SentimentClassifierBuilder()
        {
            _mlContext = new MLContext(42);
            _splitDataView = LoadData(_mlContext);
        }

        public async Task<SentimentClassifier> BuildEngine()
        {
            ITransformer model = BuildAndTrainModel(_mlContext, _splitDataView.TrainSet);

            Evaluate(_mlContext, model, _splitDataView.TestSet);

            var predictionFunction = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
            
            return new SentimentClassifier(predictionFunction);
        }

        private TrainTestData LoadData(MLContext mlContext)
        {
            IDataView dataView = mlContext.Data.LoadFromTextFile<SentimentData>(_dataPath, hasHeader: true, separatorChar: ',', allowQuoting: true, trimWhitespace: true);

            TrainTestData splitDataView = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2, seed: 0);

            return splitDataView;
        }
        private ITransformer BuildAndTrainModel(MLContext mlContext, IDataView trainSet)
        {
            var estimator = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentData.SentimentText))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

            var model = estimator.Fit(trainSet);

            return model;
        }
        private void Evaluate(MLContext mlContext, ITransformer model, IDataView testSet)
        {
            IDataView predictions = model.Transform(testSet);

            var metrics = mlContext.BinaryClassification.Evaluate(predictions, "Label");

            Console.WriteLine();
            Console.WriteLine("Model quality metrics evaluation");
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"Auc: {metrics.AreaUnderRocCurve:P2}");
            Console.WriteLine($"F1Score: {metrics.F1Score:P2}");
            Console.WriteLine("=============== End of model evaluation ===============");
        }

    }
}
