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
    @"D:\Projects\SangoResearch\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\PlayTime1.xlsx",
    @"D:\Projects\SangoResearch\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\ProcessedSet1.xlsx",
    @"D:\Projects\SangoResearch\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\ProcessedSet2.xlsx");

EMDEAlgorithm.Instance.FitNormalDistributionToCache(0, 2, 24, 4, 7,
    @"D:\Projects\SangoResearch\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\PlayTime1.xlsx");

//Thie following example will show you how to use  DMRProbability with your server.
//TODO: Get the streaming data from DataBase, such as MongoDB, the following is a simulate.

EMDEData emdeData1 = GetDMRDataRaw("mission1", 3, 1, 20);    //Too quick? Not a suitble choice
EMDEData emdeData2 = GetDMRDataRaw("mission2", 1, 1, 500);    //Tool slow? Of course should`t be recommened
EMDEData emdeData3 = GetDMRDataRaw("mission3", 2, 1, 95);
EMDEData emdeData4 = GetDMRDataRaw("mission4", 1, 2, 106);

//EMDEData emdeData1 = GetDMRDataRaw("mission1", 3, 1, 23);    //Too quick? Not a suitble choice
//EMDEData emdeData2 = GetDMRDataRaw("mission2", 1, 1, 490);    //Tool slow? Of course should`t be recommened
//EMDEData emdeData3 = GetDMRDataRaw("mission3", 2, 1, 98);
//EMDEData emdeData4 = GetDMRDataRaw("mission4", 1, 2, 103); 
//EMDEData emdeData5 = GetDMRDataRaw("mission5", 2, 2, 132);
List<EMDEData> emdeDataRaws = new List<EMDEData>() { emdeData1, emdeData2, emdeData3, emdeData4 };
//List<EMDEData> emdeDataRaws = new List<EMDEData>() { emdeData1, emdeData2, emdeData3, emdeData4, emdeData5 };
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