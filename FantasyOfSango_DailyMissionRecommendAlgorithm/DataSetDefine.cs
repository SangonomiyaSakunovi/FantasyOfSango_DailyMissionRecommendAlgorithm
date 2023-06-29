using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

//Developer: SangonomiyaSakunovi

namespace FantasyOfSango_DailyMissionRecommendAlgorithm
{
    public class DMRDataSet
    {
        [LoadColumn(0)]
        public float Mission1SuitProbability;
        [LoadColumn(1)]
        public float Mission2SuitProbability;
        [LoadColumn(2)]
        public float Mission3SuitProbability;
        [LoadColumn(3)]
        public float Mission4SuitProbability;

        [LoadColumn(4), ColumnName("Label")]
        public string Label;
    }

    public class DMRPrediction
    {
        [ColumnName("Score")]
        public float[] Score;
    }
}
