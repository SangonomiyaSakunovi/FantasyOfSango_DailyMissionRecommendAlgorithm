using SangoScripts_MaxLikehoodAndEbbinghasuAlgorithm;

//Developer: SangonomiyaSakunovi

//This config has a default value, you can define the configs customize and then Init this Instance. Good Luck!
EMDEAlgorithmConfig dMRAlgorithmConfig = new()
{
    IsCacheNormalDistribution = true,   
    PassedDaysEBHS_K_Value = 1.84f,
    PassedDaysEBHS_C_Value = 1.25f,
    PassedDaysEBHS_Weight = 9,
    DoneCountEBHS_K_Value = 1.84f,
    DoneCountEBHS_C_Value = 1.25f,
    DoneCountEBHS_Weight = 1,
};
EMDEAlgorithm dMRAlgorithm = new();
dMRAlgorithm.InitAlgorithm(dMRAlgorithmConfig);

//The following example will show you how to use DMRAlgorithm.
EMDEAlgorithm.Instance.FitNormalDistributionToExcel(0, 2, 24, 4, 7,
    @"D:\Projects\Reference\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\PlayTime.xlsx",
    @"D:\Projects\Reference\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\ProcessedSet1.xlsx",
    @"D:\Projects\Reference\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\ProcessedSet2.xlsx");

EMDEAlgorithm.Instance.FitNormalDistributionToCache(0, 2, 24, 4, 7,
    @"D:\Projects\Reference\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\PlayTime.xlsx");

//Thie following example will show you how to use  DMRProbability with your server.
//TODO: Get the streaming data from DataBase, such as MongoDB, the following is a simulate.
EMDEData emdeData1 = GetDMRDataRaw("mission1", 2, 1, 20);    //Too quick? Not a suitble choice
EMDEData emdeData2 = GetDMRDataRaw("mission2", 1, 2, 500);    //Tool slow? Of course should`t be recommened
EMDEData emdeData3 = GetDMRDataRaw("mission3", 3, 1, 95);
EMDEData emdeData4 = GetDMRDataRaw("mission4", 1, 3, 106);
List<EMDEData> emdeDataRaws = new List<EMDEData>() { emdeData1, emdeData2, emdeData3, emdeData4 };
List<EMDEData> emdeDataFits = EMDEAlgorithm.Instance.GetDMRProbabilityToList(emdeDataRaws);
for (int index = 0; index < emdeDataFits.Count; index++)
{
    EMDEData data = emdeDataFits[index];
    Console.Write(" " + data.EBSH_Probability_Fit);
}

EMDEData GetDMRDataRaw(string label, int doneCount, int passedDays, float timeReal)
{
    EMDEData data = new();
    data.Label = label;
    data.DoneCount = doneCount;
    data.PassedDays = passedDays;
    data.TimeReal = timeReal;
    return data;
}