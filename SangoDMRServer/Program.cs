using FantasyOfSango_DailyMissionRecommendAlgorithm;

//Developer: SangonomiyaSakunovi

//This config has a default value, you can define the configs customize.
//The following example will show you how to set your value, Good Luck!
DMRAlgorithmConfig dMRAlgorithmConfig = new()
{
    MongoDBName = "SangoServerGameDB",
    MongoDBAddress = "mongodb://127.0.0.1:27017",
    SaveNormalDistributionCache = true,
    PassedDaysEBHS_K_Value = 1.84f,
    PassedDaysEBHS_C_Value = 1.25f,
    DoneCountEBHS_K_Value = 1.84f,
    DoneCountEBHS_C_Value = 1.25f
};
DMRAlgorithm dMRAlgorithm = new DMRAlgorithm();
dMRAlgorithm.InitAlgorithm(dMRAlgorithmConfig);

DMRAlgorithm.Instance.FitNormalDistributionToExcel(0, 2, 24, 4, 7,
    @"E:\FantasyOfSango\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\PlayTime.xlsx",
    @"E:\FantasyOfSango\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\ProcessedSet1.xlsx",
    @"E:\FantasyOfSango\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\ProcessedSet2.xlsx");

DMRAlgorithm.Instance.FitNormalDistributionToCache(0, 2, 24, 4, 7,
    @"E:\FantasyOfSango\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\PlayTime.xlsx");



