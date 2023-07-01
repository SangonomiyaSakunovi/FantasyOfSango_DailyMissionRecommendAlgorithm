using FantasyOfSango_DailyMissionRecommendAlgorithm;

//Developer: SangonomiyaSakunovi

//This config has a default value, you can define the configs customize and then Init this Instance. Good Luck!
DMRAlgorithmConfig dMRAlgorithmConfig = new()
{
    IsCacheNormalDistribution = true,
    PassedDaysEBHS_K_Value = 1.84f,
    PassedDaysEBHS_C_Value = 1.25f,
    DoneCountEBHS_K_Value = 1.84f,
    DoneCountEBHS_C_Value = 1.25f
};
DMRAlgorithm dMRAlgorithm = new();
dMRAlgorithm.InitAlgorithm(dMRAlgorithmConfig);

//The following example will show you how to use DMRAlgorithm.
//DMRAlgorithm.Instance.FitNormalDistributionToExcel(0, 2, 24, 4, 7,
//    @"E:\FantasyOfSango\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\PlayTime.xlsx",
//    @"E:\FantasyOfSango\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\ProcessedSet1.xlsx",
//    @"E:\FantasyOfSango\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\ProcessedSet2.xlsx");

DMRAlgorithm.Instance.FitNormalDistributionToCache(0, 2, 24, 4, 7,
    @"E:\FantasyOfSango\FantasyOfSango_DailyMissionRecommendAlgorithm\Data\PlayTime.xlsx");

//Thie following example will show you how to use  DMRProbability with your server.

//TODO: Get the streaming data from DataBase, such as MongoDB, the following is a simulate.
DMRData dMRData1 = GetDMRDataRaw("mission1", 2, 1, 120);
DMRData dMRData2 = GetDMRDataRaw("mission2", 1, 2, 134);
DMRData dMRData3 = GetDMRDataRaw("mission3", 0, 1, 95);
DMRData dMRData4 = GetDMRDataRaw("mission4", 1, 1, 106);
List<DMRData> dMRDataRaws = new List<DMRData>() { dMRData1, dMRData2, dMRData3, dMRData4 };

List<DMRData> dMRDataFits = DMRAlgorithm.Instance.GetDMRProbabilityToList(dMRDataRaws);

for (int index = 0; index < dMRDataFits.Count; index++)
{
    DMRData data = dMRDataFits[index];
    Console.Write(" " + data.EBSH_Probability_Fit);
}

DMRData GetDMRDataRaw(string label, int doneCount, int passedDays, float timeReal)
{
    DMRData data = new();
    data.Label = label;
    data.DoneCount = doneCount;
    data.PassedDays = passedDays;
    data.TimeReal = timeReal;
    return data;
}