//Developer: SangonomiyaSakunovi

namespace FantasyOfSango_DailyMissionRecommendAlgorithm
{
    /// <summary>
    /// The config of DMRAlgorithm, we sugget to customize it before start
    /// </summary>
    public class DMRAlgorithmConfig
    {
        public bool IsCacheNormalDistribution = true;
        public int PassedDaysEBHS_Weight = 9;
        public int DoneCountEBHS_Weight = 1;
        public float PassedDaysEBHS_K_Value = 1.84f;
        public float PassedDaysEBHS_C_Value = 1.25f;
        public float DoneCountEBHS_K_Value = 1.84f;
        public float DoneCountEBHS_C_Value = 1.25f;
    }

    /// <summary>
    /// The DMRData of DMRAlgorithm, if want to change these, also need to redevelop our algorithm
    /// </summary>
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
