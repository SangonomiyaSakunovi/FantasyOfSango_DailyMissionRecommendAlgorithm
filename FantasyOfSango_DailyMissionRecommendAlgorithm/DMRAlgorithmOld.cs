using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;

//Developer: SangonomiyaSakunovi

namespace FantasyOfSango_DailyMissionRecommendAlgorithm
{
    /// <summary>
    /// We do not sugget to use this method, in our test, we found that this issue not suit for MachineLearning
    /// </summary>
    public class DMRAlgorithmOld
    {
        /// <summary>
        /// This method can easily use for MulticlassClassification, and need to configure the DMRDataSet and DMRPrediction before start
        /// </summary>
        /// <param name="csvPath">Define the path of DataSet, the column name should same as class DMRDataSet</param>
        /// <param name="zipPath">The trained model will save in this path, must .zip file</param>
        public void FitMulticlassClassification(string csvPath, string zipPath)
        {
            var mlContext = new MLContext();
            var trainingDataView = mlContext.Data.LoadFromTextFile<DMRDataSet>(csvPath, hasHeader: true, separatorChar: ',');

            //The Features is a special vector contain the nameof dimensions
            var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(DMRDataSet.Label))
                .Append(mlContext.Transforms.Concatenate("Features", nameof(DMRDataSet.mission1),
                                                            nameof(DMRDataSet.mission2),
                                                            nameof(DMRDataSet.mission3),
                                                            nameof(DMRDataSet.mission4))
                                                            .AppendCacheCheckpoint(mlContext));

            var trainer = mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features")
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: nameof(DMRDataSet.Label), inputColumnName: "Label"));
            var pipeline = dataProcessPipeline.Append(trainer);

            ITransformer trainedModel = pipeline.Fit(trainingDataView);

            mlContext.Model.Save(trainedModel, trainingDataView.Schema, zipPath);
            Console.WriteLine("Save model success.");
        }

        /// <summary>
        /// When we have a model, we can use this method to have a test, be careful, the TestSet should same define as TraninSet. And for using in a real server, the params here is a list.
        /// </summary>
        /// <param name="modelPath">The model which trained</param>
        /// <param name="userDataArray">The real array when the server running</param>
        public void PredictMulticlassClassification(string modelPath, List<DMRDataSet> userDataArray)
        {
            var mlContext = new MLContext();
            ITransformer trainedModel;
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream, out var inputSchema);
            }

            var predictEngine = mlContext.Model.CreatePredictionEngine<DMRDataSet, DMRPrediction>(trainedModel);

            for (int index = 0; index < userDataArray.Count; index++)
            {
                var resultprediction = predictEngine.Predict(userDataArray[index]);
                //TODO

                //Here is a example to show how to get Label, noticed that the sequence is same as TrainSet, see the Data folder
                Console.WriteLine("Predicted label and score: ");
                Console.WriteLine($"mission1:  {resultprediction.Score[0]:0.####}");
                Console.WriteLine($"mission3:  {resultprediction.Score[1]:0.####}");
                Console.WriteLine($"mission4:  {resultprediction.Score[2]:0.####}");
            }
        }

        /// <summary>
        /// A example to show how use this two method to Fit model
        /// </summary>
        private void TestMulticlassClassification()
        {
            //PreProcess the Data, see AMRAlgorithm.

            //Fit model
            FitMulticlassClassification(
                @"E:\FantasyOfSango\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\DataSet_Train.csv",
                @"E:\FantasyOfSango\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\DMRModel.zip");

            //Give the test data, when this algorithm working, it will recieve the List from GameServer, we use dMRDataSets to simulate
            DMRDataSet testdMRDataSet1 = new DMRDataSet()
            {
                mission1 = 136,
                mission2 = 144,
                mission3 = 95,
                mission4 = 110
            };
            DMRDataSet testdMRDataSet2 = new DMRDataSet()
            {
                mission1 = 135,
                mission2 = 144,
                mission3 = 127,
                mission4 = 110
            };
            List<DMRDataSet> dMRDataSets = new List<DMRDataSet>() { testdMRDataSet1, testdMRDataSet2 };

            //Predict model
            PredictMulticlassClassification(
                @"E:\FantasyOfSango\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\DMRModel.zip",
                dMRDataSets);
        }

        /// <summary>
        /// The data set define
        /// </summary>
        public class DMRDataSet
        {
            [LoadColumn(0)]
            public float mission1 { get; set; }
            [LoadColumn(1)]
            public float mission2 { get; set; }
            [LoadColumn(2)]
            public float mission3 { get; set; }
            [LoadColumn(3)]
            public float mission4 { get; set; }

            [LoadColumn(4), ColumnName("Label")]
            public float Label { get; set; }
        }

        /// <summary>
        /// The predict class, after prediction, the Score.Length == TrainSet.Label.Length
        /// </summary>
        public class DMRPrediction
        {
            public float[] Score { get; set; }
        }
    }
}
