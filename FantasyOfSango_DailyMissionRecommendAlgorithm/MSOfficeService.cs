using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;

//Developer: SangonomiyaSakunovi

namespace FantasyOfSango_DailyMissionRecommendAlgorithm
{
    public class MSOfficeService
    {
        public static List<string> LoadExcelColumnName(int sheetNum, int startColumn, int endColumn, string path)
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

        public static List<List<float>> LoadExcelMultiFloatValue(int sheetNum, int startRow, int endRow, int startColumn, int endColumn, string path)
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

        public static List<List<string>> LoadExcelMultiStringValue(int sheetNum, int startRow, int endRow, int startColumn, int endColumn, string path)
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

        public static void WriteExcelMultiObjectValue(List<List<string>> data, string path)
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
    }
}
