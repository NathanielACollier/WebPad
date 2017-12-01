using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Dependencies.General.EPPlusUtilities
{

    public static class ExcelReadUtility
    {
        //http://stackoverflow.com/questions/4538321/reading-datetime-value-from-excel-sheet
        public static DateTime? ReadExcelOLEAutomationDateColumn(string dateColumnText)
        {
            double oleADateNumberVal;

            if (double.TryParse(dateColumnText, out oleADateNumberVal))
            {
                return DateTime.FromOADate(oleADateNumberVal);
            }

            return null;
        }


        public static object ReadCell(OfficeOpenXml.ExcelWorksheet worksheet, string column, int rowNumber)
        {
            object val = worksheet.Cells[column + rowNumber].Value;

            return val;
        }

        public static object ReadCell(OfficeOpenXml.ExcelWorksheet worksheet, int rowNumber, int columnNum)
        {
            object val = worksheet.Cells[rowNumber, columnNum].Value;

            return val;
        }

        public static string ReadCellAsString(OfficeOpenXml.ExcelWorksheet worksheet, int rowNum, int columnNum)
        {
            object cellValue = ReadCell(worksheet, rowNum, columnNum);

            if (cellValue == null)
            {
                return "";
            }
            else
            {
                return cellValue.ToString();
            }
        }

    }// end of class
}
