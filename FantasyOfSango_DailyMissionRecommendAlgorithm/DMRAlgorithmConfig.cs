//Developer: SangonomiyaSakunovi

namespace FantasyOfSango_DailyMissionRecommendAlgorithm
{
    public class DMRAlgorithmConfig
    {
        public string MongoDBName = "SangoServerGameDB";
        public string MongoDBAddress = "mongodb://127.0.0.1:27017";
        public bool SaveNormalDistributionCache = true;
        public float PassedDaysEBHS_K_Value = 1.84f;
        public float PassedDaysEBHS_C_Value = 1.25f;
        public float DoneCountEBHS_K_Value = 1.84f;
        public float DoneCountEBHS_C_Value = 1.25f;
    }

    public struct DMRData
    {
        public string Label { get; set; }
        public int DoneCount { get; set; }
        public int PassedDays { get; set; }
        public float TimeReal { get; set; }
        public float NormalProbabilityRaw { get; set; }
        public float NormalProbabilityFit { get; set; }
        public float EBHS_PassedDay_Probability { get; set; }
        public float EBHS_DoneCount_Probability { get; set; }
        public float EBSH_Probability_Raw { get; set; }
        public float EBSH_Probability_Fit { get; set; }
    }
}
