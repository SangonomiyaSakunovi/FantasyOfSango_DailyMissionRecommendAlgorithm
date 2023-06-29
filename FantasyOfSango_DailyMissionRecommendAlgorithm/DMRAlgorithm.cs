using MathNet.Numerics.Distributions;
using Microsoft.ML;
using System;
using System.Collections.Generic;

//Developer: SangonomiyaSakunovi

namespace FantasyOfSango_DailyMissionRecommendAlgorithm
{
    public class DMRAlgorithm
    {
        public static DMRAlgorithm Instance;

        public void InitAlgorithm()
        {
            Instance = this;
        }

        string[] inputArray = { "Mission1SuitProbability", "Mission2SuitProbability", "Mission3SuitProbability", "Mission4SuitProbability" };

        public void RunMulticlassClassification(string path, string[] inputArray)
        {
            var context = new MLContext();
            var data = context.Data.LoadFromTextFile<DMRDataSet>(path, separatorChar: ',');
            var trainTestSplit = context.Data.TrainTestSplit(data);

            //"Features" is a special params which can combine all the params in inputArray into one vector
            //The trainer will use this vector to train this model
            var dataProcessPipeline = context.Transforms.Conversion.MapValueToKey("Label")
                .Append(context.Transforms.Concatenate("Features", inputArray))
                .Append(context.Transforms.NormalizeMinMax("Features"));
            var trainer = context.MulticlassClassification.Trainers.SdcaNonCalibrated();
            var pipeline = dataProcessPipeline.Append(trainer)
                .Append(context.Transforms.Conversion.MapKeyToValue("PredictLabel"));

            //Train the model
            var model = pipeline.Fit(trainTestSplit.TrainSet);

            //Predict and Evaluate the model
            var predictions = model.Transform(trainTestSplit.TestSet);
            var metrics = context.MulticlassClassification.Evaluate(predictions);

            Console.WriteLine($"The MacroAccuracy of this model is: {metrics.MacroAccuracy}");

            string consoleInput = Console.ReadLine();
            if (consoleInput == "Save")
            {
                context.Model.Save(model, data.Schema, "softmax-regression-model.zip");
                Console.WriteLine("Save model success.");
            }
        }

        public void PredictMulticlassClassification(string path, List<DMRDataSet> userDMRDataSetArray)
        {
            var context = new MLContext();
            var model = context.Model.Load(path, out var modelSchema);
            var predictionEngine = context.Model.CreatePredictionEngine<DMRDataSet, DMRPrediction>(model);

            for (int i = 0; i < userDMRDataSetArray.Count; i++)
            {
                var prediction = predictionEngine.Predict(userDMRDataSetArray[i]);
                var probabilities = prediction.Score;
                Console.WriteLine("The prediction result distribution:\n");
                foreach (var label in modelSchema.GetColumnOrNull("Label").Value.Annotations.Schema)
                {
                    var columnName = label.Name;
                    var probability = probabilities[label.Index];
                    Console.WriteLine($"{columnName}: {probability}");
                }
            }
        }

        double[] data = { 1.5, 2.7, 3.9, 4.2, 5.1 };

        public void NormalDistribution(double[] data)
        {
            double mean = CalculateMean(data);
            double standardDeviation = CalculateStandardDeviation(data, mean);

            foreach (var value in data)
            {
                double probability = Normal.PDF(mean, standardDeviation, value);
                Console.WriteLine($"数据点 {value} 的正态分布概率为: {probability}");
            }

        }

        private double CalculateMean(double[] data)
        {
            double sum = 0;
            foreach (var value in data)
            {
                sum += value;
            }
            return sum / data.Length;
        }

        private double CalculateStandardDeviation(double[] data, double mean)
        {
            double sumOfSquaredDifferences = 0;
            foreach (var value in data)
            {
                double difference = value - mean;
                sumOfSquaredDifferences += difference * difference;
            }
            return Math.Sqrt(sumOfSquaredDifferences / data.Length);
        }

        public void EbbinghausForgettingCurve()
        {
            int timePassedInDays = 10; // 经过的时间（以天为单位）
            double retention = CalculateRetention(timePassedInDays);
            Console.WriteLine($"经过 {timePassedInDays} 天后的保留率为: {retention}");
        }

        private double CalculateRetention(int timePassedInDays)
        {
            double k = 1.84; // 常量k
            double c = 1.25; // 常量c
            double logarithm = Math.Log(timePassedInDays);
            return (100 * k) / (Math.Pow(logarithm, c) + k);
        }
    }
}
