using MathNet.Numerics.Distributions;
using OfficeOpenXml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

//Developer: SangonomiyaSakunovi

namespace FantasyOfSango_DailyMissionRecommendAlgorithm
{
    /// <summary>
    /// This is the main DMRAlgorithm class
    /// </summary>
    public class DMRAlgorithm
    {
        /// <summary>
        /// All the DMR method is under this Instance
        /// </summary>
        public static DMRAlgorithm Instance;

        private DMRAlgorithmConfig dMRConfig;
        private ConcurrentDictionary<string, Normal> cacheNormalDistributionDict;

        /// <summary>
        /// The Init method, must call before use other method
        /// </summary>
        /// <param name="dMRAlgorithmConfig">We suggest customize it before init</param>
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
            if (dMRConfig.IsCacheNormalDistribution)
            {
                cacheNormalDistributionDict = new ConcurrentDictionary<string, Normal>();
            }
        }

        /// <summary>
        /// Auto fit the normal distribution, we suggest just use it, do not need to care how it works
        /// </summary>
        /// <param name="sheetNum">The sheet Num in the readExcel, start is 0</param>
        /// <param name="startRow">The start row in the sheet, start is 1</param>
        /// <param name="endRow">The end row in the sheet, start is 1</param>
        /// <param name="startColumn">The start column in the sheet, start is 1</param>
        /// <param name="endColumn">The end column in the sheet, start is 1</param>
        /// <param name="readPath">The full path of the readExcel</param>
        /// <param name="write1Path">The full path of the normal distribution Excel</param>
        /// <param name="write2Path">The full path of the raw value Excel, but in this file, append the Label by normal distribution</param>
        public void FitNormalDistributionToExcel(int sheetNum, int startRow, int endRow, int startColumn, int endColumn, string readPath, string write1Path, string write2Path)
        {
            List<string> columnNames = LoadExcelColumnName(sheetNum, startColumn, endColumn, readPath);
            List<List<float>> floats = LoadExcelMultiFloatValue(sheetNum, startRow, endRow, startColumn, endColumn, readPath);
            List<List<string>> fitData = FitColumnNamesAndNormalDistributionToList(columnNames, floats);
            List<string> fitLabel = FitLabelByMaxNormalDistributionProbability(fitData);

            fitData.Add(fitLabel);
            WriteExcelMultiObjectValue(fitData, write1Path);

            List<List<string>> rawData = LoadExcelMultiStringValue(sheetNum, startRow - 1, endRow, startColumn, endColumn, readPath);
            rawData.Add(fitLabel);
            WriteExcelMultiObjectValue(rawData, write2Path);

            Console.WriteLine("Excel file created successfully.");
        }

        /// <summary>
        /// Auto fit the normal distribution,  we suggest just use it, do not need to care how it works
        /// </summary>
        /// <param name="sheetNum">The sheet Num in the readExcel, start is 0</param>
        /// <param name="startRow">The start row in the sheet, start is 1</param>
        /// <param name="endRow">The end row in the sheet, start is 1</param>
        /// <param name="startColumn">The start column in the sheet, start is 1</param>
        /// <param name="endColumn">The end column in the sheet, start is 1</param>
        /// <param name="readPath">The full path of the readExcel</param>
        public void FitNormalDistributionToCache(int sheetNum, int startRow, int endRow, int startColumn, int endColumn, string readPath)
        {
            List<string> columnNames = LoadExcelColumnName(sheetNum, startColumn, endColumn, readPath);
            List<List<float>> floats = LoadExcelMultiFloatValue(sheetNum, startRow, endRow, startColumn, endColumn, readPath);
            FitColumnNamesAndNormalDistributionToDict(columnNames, floats);
        }

        /// <summary>
        /// Auto get the DMRProbability, and it can use in server, we also suggest just use it, do not need to care how it works
        /// </summary>
        /// <param name="userDatas">One user data for all mission conditions</param>
        /// <returns>DMRData with all probability</returns>
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
                if (data.DoneCount < minDoneCount)
                {
                    minDoneCount = data.DoneCount;
                }
                userDatas[index] = data;
            }
            for (int index = 0; index < userDatas.Count; index++)
            {
                DMRData data = userDatas[index];
                int differDoneCount = data.DoneCount - minDoneCount;
                data.NormalProbabilityFit = data.NormalProbabilityRaw / probRawSum;
                float dayRetention = CalculateEbbinghausRetention(dMRConfig.PassedDaysEBHS_K_Value, dMRConfig.PassedDaysEBHS_C_Value, data.PassedDays);
                data.EBHS_PassedDay_Probability = data.NormalProbabilityFit * (1 - dayRetention);
                float countRetention = CalculateEbbinghausRetention(dMRConfig.DoneCountEBHS_K_Value, dMRConfig.DoneCountEBHS_C_Value, differDoneCount);
                data.EBHS_DoneCount_Probability = data.NormalProbabilityFit * countRetention;
                data.EBSH_Probability_Raw = (data.EBHS_PassedDay_Probability * dMRConfig.PassedDaysEBHS_Weight + data.EBHS_DoneCount_Probability * dMRConfig.DoneCountEBHS_Weight) /
                    (dMRConfig.PassedDaysEBHS_Weight + dMRConfig.DoneCountEBHS_Weight);
                probEBSH_Sum += data.EBSH_Probability_Raw;
                userDatas[index] = data;
            }
            for (int index = 0; index < userDatas.Count; index++)
            {
                DMRData data = userDatas[index];
                data.EBSH_Probability_Fit = data.EBSH_Probability_Raw / probEBSH_Sum;
                userDatas[index] = data;
            }
            return userDatas;
        }

        #region FitColumns
        private List<List<string>> FitColumnNamesAndNormalDistributionToList(List<string> columnNames, List<List<float>> floats)
        {
            if (dMRConfig.IsCacheNormalDistribution)
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
            if (dMRConfig.IsCacheNormalDistribution)
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
            float density = (float)normal.Density(data);
            return density;
        }

        private float CalculateMean(List<float> data)
        {
            float sum = 0;
            for (int i = 0; i < data.Count; i++)
            {
                sum += data[i];
            }
            float mean = sum / data.Count;
            return mean;
        }

        private float CalculateStandardDeviation(List<float> data, float mean)
        {
            float sumOfSquaredDifferences = 0;
            for (int i = 0; i < data.Count; i++)
            {
                float difference = data[i] - mean;
                sumOfSquaredDifferences += difference * difference;
            }
            float stdDev = (float)Math.Sqrt(sumOfSquaredDifferences / data.Count);
            return stdDev;
        }
        #endregion

        #region FitEbbinghas
        private float CalculateEbbinghausRetention(float k, float c, int timePassedInDays)
        {
            double logarithm = Math.Log(timePassedInDays);
            float ebhs = (float)(k / (Math.Pow(logarithm, c) + k));
            return ebhs;
        }
        #endregion

        #region MSOffice
        private List<string> LoadExcelColumnName(int sheetNum, int startColumn, int endColumn, string path)
        {
            List<string> columns = new List<string>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage(new FileInfo(path)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetNum];
                int rowNumber = 1;
                ExcelRangeBase columnRange = worksheet.Cells[rowNumber, startColumn, rowNumber, endColumn];
                foreach (var cell in columnRange)
                {
                    string cellValue = cell.Value.ToString();
                    columns.Add(cellValue);
                }
            }
            return columns;
        }

        private List<List<float>> LoadExcelMultiFloatValue(int sheetNum, int startRow, int endRow, int startColumn, int endColumn, string path)
        {
            List<List<float>> floats = new List<List<float>>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage(new FileInfo(path)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetNum];
                for (int col = startColumn; col <= endColumn; col++)
                {
                    List<float> tempFloat = new List<float>();
                    int columnNumber = col;
                    ExcelRangeBase columnRange = worksheet.Cells[startRow, columnNumber, endRow, columnNumber];
                    foreach (var cell in columnRange)
                    {
                        float cellValue = float.Parse(cell.Value.ToString());
                        tempFloat.Add(cellValue);
                    }
                    floats.Add(tempFloat);
                }
            }
            return floats;
        }

        private List<List<string>> LoadExcelMultiStringValue(int sheetNum, int startRow, int endRow, int startColumn, int endColumn, string path)
        {
            List<List<string>> strings = new List<List<string>>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage(new FileInfo(path)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetNum];
                for (int col = startColumn; col <= endColumn; col++)
                {
                    List<string> temp = new List<string>();
                    int columnNumber = col;
                    ExcelRangeBase columnRange = worksheet.Cells[startRow, columnNumber, endRow, columnNumber];
                    foreach (var cell in columnRange)
                    {
                        string cellValue = cell.Value.ToString();
                        temp.Add(cellValue);
                    }
                    strings.Add(temp);
                }
            }
            return strings;
        }

        private void WriteExcelMultiObjectValue(List<List<string>> data, string path)
        {
            var newFile = new FileInfo(path);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(newFile))
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                for (var col = 0; col < data[0].Count; col++)
                {
                    for (var row = 0; row < data.Count; row++)
                    {
                        worksheet.Cells[col + 1, row + 1].Value = data[row][col];
                    }
                }
                package.Save();
            }
        }
        #endregion
    }
}