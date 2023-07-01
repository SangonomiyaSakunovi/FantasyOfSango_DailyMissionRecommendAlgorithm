using MathNet.Numerics.Distributions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

//Developer: SangonomiyaSakunovi

namespace FantasyOfSango_DailyMissionRecommendAlgorithm
{
    public class DMRAlgorithm
    {
        public static DMRAlgorithm Instance;
        
        private DMRAlgorithmConfig dMRConfig;
        private ConcurrentDictionary<string, Normal> cacheNormalDistributionDict;

        public void InitAlgorithm(DMRAlgorithmConfig dMRAlgorithmConfig = null)
        {
            Instance = this;
            if (dMRAlgorithmConfig == null)
            {
                dMRAlgorithmConfig = new DMRAlgorithmConfig();
            }
            dMRConfig = dMRAlgorithmConfig;
            InitSettings();
        }

        private void InitSettings()
        {
            MongoDBService mongoDBService = new MongoDBService();
            mongoDBService.InitService(dMRConfig.MongoDBName, dMRConfig.MongoDBAddress);
            if (dMRConfig.SaveNormalDistributionCache)
            {
                cacheNormalDistributionDict = new ConcurrentDictionary<string, Normal>();
            }
        }

        public void FitNormalDistributionToExcel(int sheetNum, int startRow, int endRow, int startColumn, int endColumn, string readPath, string write1Path, string write2Path)
        {
            List<string> columnNames = MSOfficeService.LoadExcelColumnName(sheetNum, startColumn, endColumn, readPath);
            List<List<float>> floats = MSOfficeService.LoadExcelMultiFloatValue(sheetNum, startRow, endRow, startColumn, endColumn, readPath);
            List<List<string>> fitData = FitColumnNamesAndNormalDistributionToList(columnNames, floats);
            List<string> fitLabel = FitLabelByMaxNormalDistributionProbability(fitData);

            fitData.Add(fitLabel);
            MSOfficeService.WriteExcelMultiObjectValue(fitData, write1Path);

            List<List<string>> rawData = MSOfficeService.LoadExcelMultiStringValue(sheetNum, startRow - 1, endRow, startColumn, endColumn, readPath);
            rawData.Add(fitLabel);
            MSOfficeService.WriteExcelMultiObjectValue(rawData, write2Path);

            Console.WriteLine("Excel file created successfully.");
        }

        public void FitNormalDistributionToCache(int sheetNum, int startRow, int endRow, int startColumn, int endColumn, string readPath)
        {
            List<string> columnNames = MSOfficeService.LoadExcelColumnName(sheetNum, startColumn, endColumn, readPath);
            List<List<float>> floats = MSOfficeService.LoadExcelMultiFloatValue(sheetNum, startRow, endRow, startColumn, endColumn, readPath);
            FitColumnNamesAndNormalDistributionToDict(columnNames, floats);
        }

        public List<DMRData> GetDMRProbabilityToList(List<DMRData> userDatas)
        {
            float probRawSum = 0;
            float probEBSH_Sum = 0;
            int minDoneCount = userDatas[0].DoneCount;
            for (int index = 0; index < userDatas.Count; index++)
            {
                DMRData data = userDatas[index];
                Normal normal;
                cacheNormalDistributionDict.TryGetValue(data.Label, out normal);
                if (normal != null)
                {
                    data.NormalProbabilityRaw = GetNormalDistributionDensity(normal, data.TimeReal);
                    probRawSum += data.NormalProbabilityRaw;
                }
                if (userDatas[index].DoneCount < minDoneCount)
                {
                    minDoneCount = userDatas[index].DoneCount;
                }
            }
            for (int index = 0; index < userDatas.Count; index++)
            {
                DMRData data = userDatas[index];
                int differDoneCount = data.DoneCount - minDoneCount;
                data.NormalProbabilityFit = data.NormalProbabilityRaw / probRawSum;
                data.EBHS_PassedDay_Probability = data.NormalProbabilityFit * CalculateEbbinghausRetention(dMRConfig.PassedDaysEBHS_K_Value, dMRConfig.PassedDaysEBHS_C_Value, data.PassedDays);
                data.EBHS_DoneCount_Probability = data.NormalProbabilityFit * CalculateEbbinghausRetention(dMRConfig.DoneCountEBHS_K_Value, dMRConfig.DoneCountEBHS_C_Value, differDoneCount);
                data.EBSH_Probability_Raw = (data.EBHS_PassedDay_Probability + data.EBHS_DoneCount_Probability) / 2;
                probEBSH_Sum += data.EBSH_Probability_Raw;
            }
            for (int index = 0; index < userDatas.Count; index++)
            {
                DMRData data = userDatas[index];
                data.EBSH_Probability_Fit = data.EBSH_Probability_Raw / probEBSH_Sum;
            }
            return userDatas;
        }

        #region FitColumns
        private List<List<string>> FitColumnNamesAndNormalDistributionToList(List<string> columnNames, List<List<float>> floats)
        {
            if (dMRConfig.SaveNormalDistributionCache)
            {
                cacheNormalDistributionDict.Clear();
            }
            List<List<string>> results = new List<List<string>>();
            for (int index = 0; index < columnNames.Count; index++)
            {
                List<string> tempRes = new List<string>() { columnNames[index] };
                tempRes = FitNormalDistributionToList(floats[index], tempRes);
                results.Add(tempRes);
            }
            return results;
        }

        private void FitColumnNamesAndNormalDistributionToDict(List<string> columnNames, List<List<float>> floats)
        {
            cacheNormalDistributionDict.Clear();
            for (int index = 0; index < columnNames.Count; index++)
            {
                Normal tempNormal = FitNormalDistribution(floats[index]);
                cacheNormalDistributionDict.TryAdd(columnNames[index], tempNormal);
            }
        }

        private List<string> FitLabelByMaxNormalDistributionProbability(List<List<string>> data)
        {
            List<string> tempData = new List<string>() { "Label" };
            int columnNum = data.Count;
            for (int index = 1; index < data[0].Count; index++)
            {
                float max = float.Parse(data[0][index]);
                string colName = data[0][0];
                for (int col = 1; col < columnNum; col++)
                {
                    float temp = float.Parse(data[col][index]);
                    if (temp > max)
                    {
                        max = temp;
                        colName = data[col][0];
                    }
                }
                tempData.Add(colName);
            }
            return tempData;
        }
        #endregion

        #region FitNormalDistribution
        private List<string> FitNormalDistributionToList(List<float> data, List<string> tempRes)
        {
            float mean = CalculateMean(data);
            float standardDeviation = CalculateStandardDeviation(data, mean);
            Normal normalDistribution = new Normal(mean, standardDeviation);
            if (dMRConfig.SaveNormalDistributionCache)
            {
                cacheNormalDistributionDict.TryAdd(tempRes[0], normalDistribution);
            }
            for (int i = 0; i < data.Count; i++)
            {
                string probability = normalDistribution.Density(data[i]).ToString();
                tempRes.Add(probability);
            }
            return tempRes;
        }

        private Normal FitNormalDistribution(List<float> data)
        {
            float mean = CalculateMean(data);
            float standardDeviation = CalculateStandardDeviation(data, mean);
            Normal normalDistribution = new Normal(mean, standardDeviation);
            return normalDistribution;
        }

        private float GetNormalDistributionDensity(Normal normal, float data)
        {
            return (float)normal.Density(data);
        }

        private float CalculateMean(List<float> data)
        {
            float sum = 0;
            for (int i = 0; i < data.Count; i++)
            {
                sum += data[i];
            }
            return sum / data.Count;
        }

        private float CalculateStandardDeviation(List<float> data, float mean)
        {
            float sumOfSquaredDifferences = 0;
            for (int i = 0; i < data.Count; i++)
            {
                float difference = data[i] - mean;
                sumOfSquaredDifferences += difference * difference;
            }
            return (float)Math.Sqrt(sumOfSquaredDifferences / data.Count);
        }
        #endregion

        #region FitEbbinghas
        private float CalculateEbbinghausRetention(float k, float c, int timePassedInDays)
        {
            double logarithm = Math.Log(timePassedInDays);
            return (float)((100 * k) / (Math.Pow(logarithm, c) + k));
        }
        #endregion
    }
}