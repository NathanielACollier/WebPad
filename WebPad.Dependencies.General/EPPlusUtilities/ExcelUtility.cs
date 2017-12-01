using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebPad.Dependencies.General.EPPlusUtilities
{
    public static class ExcelUtility
    {

        public static List<string> GenerateExcelColumnNamesBetweenTwoColumnsInclusive(string columnStart, string columnEnd)
        {
            List<string> columns = new List<string>();


            int startingColumnNumber = GetNumberFromExcelColumn(columnStart);
            int endingColumnNumber = GetNumberFromExcelColumn(columnEnd);

            for (int columnNumber = startingColumnNumber; columnNumber <= endingColumnNumber; ++columnNumber)
            {
                columns.Add(GetExcelColumnFromNumber(columnNumber));
            }

            return columns;
        }




        /// <summary>
        /// From: http://stackoverflow.com/questions/837155/fastest-function-to-generate-excel-column-letters-in-c-sharp
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string GetExcelColumnFromNumber(int column)
        {
            string columnString = "";
            decimal columnNumber = column;
            while (columnNumber > 0)
            {
                decimal currentLetterNumber = (columnNumber - 1) % 26;
                char currentLetter = (char)(currentLetterNumber + 65);
                columnString = currentLetter + columnString;
                columnNumber = (columnNumber - (currentLetterNumber + 1)) / 26;
            }
            return columnString;
        }


        /// <summary>
        /// From: http://stackoverflow.com/questions/837155/fastest-function-to-generate-excel-column-letters-in-c-sharp
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static int GetNumberFromExcelColumn(string column)
        {
            int retVal = 0;
            string col = column.ToUpper();
            for (int iChar = col.Length - 1; iChar >= 0; iChar--)
            {
                char colPiece = col[iChar];
                int colNum = colPiece - 64;
                retVal = retVal + colNum * (int)Math.Pow(26, col.Length - (iChar + 1));
            }
            return retVal;
        }



        public static int? FindColumnIndexByValueIgnoreCase(ExcelWorksheet sheet, int row, string columnValue)
        {
            if (row < 1 || row > sheet.Dimension.End.Row)
            {
                throw new Exception($"Row [{row}] is either less than 1, or it's greater than last row of [{sheet.Dimension.End.Row}]");
            }

            for (int col = 1; col <= sheet.Dimension.End.Column; ++col)
            {
                var val = sheet.Cells[row, col].Value.ToString();
                if (string.Equals(val, columnValue, StringComparison.OrdinalIgnoreCase))
                {
                    return col;
                }
            }

            return null;
        }


        public static Dictionary<string, int> FindAllColumnValueToIndexCaseInsensitive(ExcelWorksheet sheet, int row)
        {
            var dict = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

            if (row < 1 || row > sheet.Dimension.End.Row)
            {
                throw new Exception($"Row [{row}] is either less than 1, or it's greater than last row of [{sheet.Dimension.End.Row}]");
            }

            for (int col = 1; col <= sheet.Dimension.End.Column; ++col)
            {
                var val = sheet.Cells[row, col].Value.ToString();
                dict[val] = col;
            }

            return dict;
        }


    }
}
