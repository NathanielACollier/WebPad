﻿using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;// DataTable
using System.Linq;
using System.Text;

namespace WebPad.Dependencies.General.EPPlusUtilities
{

    public static class ExcelDataTableUtility
    {
        /// <summary>
        /// Original code from: http://stackoverflow.com/questions/13669733/export-datatable-to-excel-with-epplus
        /// </summary>
        /// <param name="filePath"></param>
        public static void SaveDataTableToExcel(string filePath, DataTable table, bool printHeader = false, string worksheetName = "data")
        {
            var fileInfo = new ZetaLongPaths.ZlpFileInfo(filePath);
            var handle = fileInfo.CreateHandle(ZetaLongPaths.Native.CreationDisposition.CreateAlways, ZetaLongPaths.Native.FileAccess.GenericAll, ZetaLongPaths.Native.FileShare.Delete);

            using (var fs = new System.IO.FileStream(handle, System.IO.FileAccess.Write))
            {
                SaveDataTableToExcel(fs, table, printHeader, worksheetName);
            }
        }

        public static byte[] SaveDataTableToExcel(DataTable table, bool printHeader = false, string worksheetName = "data")
        {
            using (var ms = new System.IO.MemoryStream())
            {
                SaveDataTableToExcel(ms, table, printHeader, worksheetName);

                return ms.ToArray();
            }
        }

        public static void SaveDataTableToExcel(System.IO.Stream destinationStream, DataTable table, bool printHeader = false, string worksheetName = "data")
        {
            using (ExcelPackage pkg = new ExcelPackage(destinationStream))
            {
                ExcelWorksheet ws = pkg.Workbook.Worksheets.Add(worksheetName);

                ws.Cells["A1"].LoadFromDataTable(table, printHeader);

                pkg.Save();
            }
        }

        private static System.IO.FileStream CreateReadOnlyFileStream(string filePath)
        {
            var fileInfo = new ZetaLongPaths.ZlpFileInfo(filePath);
            var handle = fileInfo.CreateHandle(ZetaLongPaths.Native.CreationDisposition.OpenExisting,
                                            ZetaLongPaths.Native.FileAccess.GenericRead,
                                            ZetaLongPaths.Native.FileShare.Read);

            return new System.IO.FileStream(handle, System.IO.FileAccess.Read);
        }


        public static DataTable TransformWorksheetToDataTable(string excelFilePath,
                                                 string worksheetName,
                                                bool firstRowIsHeader = false,
                                                Model.ColumnNameType columnType = Model.ColumnNameType.CellValue)
        {

            using (var stream = CreateReadOnlyFileStream(excelFilePath))
            {
                DataTable table = TransformWorksheetToDataTable(stream, worksheetName, firstRowIsHeader, columnType);

                return table;
            }
        }

        public static DataTable TransformWorksheetToDataTable(string excelFilePath,
                                         int worksheetPosition = 1, // workbook positions start at 1
                                        bool firstRowIsHeader = false,
                                            Model.ColumnNameType columnType = Model.ColumnNameType.CellValue)
        {
            using (var stream = CreateReadOnlyFileStream(excelFilePath))
            {
                DataTable table = TransformWorksheetToDataTable(stream, worksheetPosition, firstRowIsHeader, columnType);

                return table;
            }
        }

        public static DataTable TransformWorksheetToDataTable(System.IO.Stream excelSpreadsheetStream,
                                                                            int worksheetPosition = 1,
                                                                            bool firstRowIsHeader = false,
                                                                            Model.ColumnNameType columnType = Model.ColumnNameType.CellValue)
        {
            using (OfficeOpenXml.ExcelPackage pkg = new OfficeOpenXml.ExcelPackage(excelSpreadsheetStream))
            {
                DataTable table = TransformWorksheetToDataTable(pkg, worksheetPosition, firstRowIsHeader, columnType);

                return table;
            }
        }

        /// <summary>
        /// This is mostly just for testing.  You should be streaming the data in, I've just had problems with that in the past
        /// </summary>
        /// <param name="excelFileData"></param>
        /// <param name="worksheetPosition"></param>
        /// <param name="firstRowIsHeader"></param>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public static DataTable TransformWorksheetToDataTable(byte[] excelFileData,
                                                                    int worksheetPosition = 1,
                                                                    bool firstRowIsHeader = false,
                                                                    Model.ColumnNameType columnType = Model.ColumnNameType.CellValue)
        {
            using (var ms = new System.IO.MemoryStream(excelFileData))
            {
                return TransformWorksheetToDataTable(ms, worksheetPosition, firstRowIsHeader, columnType);
            }
        }

        public static DataTable TransformWorksheetToDataTable(byte[] excelFileData, string worksheetName, bool firstRowIsHeader = false, Model.ColumnNameType columnType = Model.ColumnNameType.CellValue)
        {
            using (var ms = new System.IO.MemoryStream(excelFileData))
            {
                return TransformWorksheetToDataTable(ms, worksheetName: worksheetName, firstRowIsHeader: firstRowIsHeader, columnType: columnType);
            }
        }

        public static DataTable TransformWorksheetToDataTable(System.IO.Stream excelSpreadsheetStream, string worksheetName, bool firstRowIsHeader = false, Model.ColumnNameType columnType = Model.ColumnNameType.CellValue)
        {
            using (OfficeOpenXml.ExcelPackage pkg = new OfficeOpenXml.ExcelPackage(excelSpreadsheetStream))
            {
                DataTable table = TransformWorksheetToDataTable(pkg, worksheetName, firstRowIsHeader, columnType);

                return table;
            }
        }

        public static DataTable TransformWorksheetToDataTable(ExcelPackage package, string worksheetName, bool firstRowIsHeader = false, Model.ColumnNameType columnType = Model.ColumnNameType.CellValue)
        {
            var worksheet = package.Workbook.Worksheets[worksheetName];

            DataTable table = TransformWorksheetToDataTable(worksheet, firstRowIsHeader, columnType);

            return table;
        }

        public static DataTable TransformWorksheetToDataTable(ExcelPackage package, int worksheetPosition = 1, bool firstRowIsHeader = false, Model.ColumnNameType columnType = Model.ColumnNameType.CellValue)
        {
            var worksheet = package.Workbook.Worksheets[worksheetPosition];

            DataTable table = TransformWorksheetToDataTable(worksheet, firstRowIsHeader, columnType);

            return table;
        }

        public static DataTable TransformWorksheetToDataTable(ExcelWorksheet worksheet, bool firstRowIsHeader = false, Model.ColumnNameType columnType = Model.ColumnNameType.CellValue)
        {
            var targetRange = worksheet.Cells[worksheet.Dimension.Address];

            DataTable table = TransformRangeToDataTable(targetRange, firstRowIsHeader, columnType);

            return table;
        }


        public static DataTable TransformTableToDataTable(OfficeOpenXml.Table.ExcelTable table)
        {
            var cells = table.WorkSheet.Cells;

            var firstRow = table.Address.Start.Row;
            DataTable result = new DataTable();
            DataRow resultRow = null;

            for (int r = table.Address.Start.Row; r <= table.Address.End.Row; r++)
            {
                if (!table.ShowHeader || r != firstRow)
                {
                    // start a new row because we don't have a header or this isn't the first row
                    // If there is no header it doesn't matter if we are on the first row
                    resultRow = result.NewRow();
                }
                int resultColumnIndex = 0;
                // columns
                for (int c = table.Address.Start.Column; c <= table.Address.End.Column; c++)
                {
                    var cell = table.WorkSheet.Cells[r, c];

                    if (table.ShowHeader && r == firstRow)
                    {
                        // val is header name
                        result.Columns.Add(cell.Value as string, typeof(object));
                    }
                    else
                    {
                        // val is cell value
                        resultRow[resultColumnIndex] = cell.Value;
                        ++resultColumnIndex;
                    }
                }

                // might be the header row
                if (resultRow != null)
                {
                    result.Rows.Add(resultRow);
                }
            }

            return result;
        }






        public static DataTable TransformRangeToDataTable(ExcelRange range,
                                                    bool firstRowIsHeader = false,
                                                    Model.ColumnNameType columnType = Model.ColumnNameType.CellValue)
        {
            DataTable dt = new DataTable();


            int rowStart = range.Start.Row;

            // form columns for the data table
            for (int columnNumber = range.Start.Column; columnNumber <= range.End.Column; ++columnNumber)
            {
                string excelColumnAddressName = ExcelUtility.GetExcelColumnFromNumber(columnNumber);
                string cellValueAsString = ExcelReadUtility.ReadCellAsString(range.Worksheet, rowStart, columnNumber);

                if (firstRowIsHeader == true &&
                    columnType == Model.ColumnNameType.CellValue &&
                    !string.IsNullOrEmpty(cellValueAsString)
                    )
                {
                    DataColumn column = new DataColumn(cellValueAsString, typeof(object));
                    dt.Columns.Add(column);
                }
                else
                {
                    // use excel column address name
                    DataColumn column = new DataColumn(excelColumnAddressName, typeof(object));
                    dt.Columns.Add(column);
                }
            }

            if (firstRowIsHeader == true)
            {
                // take row 1 as a header
                ++rowStart; // which means data will start on the next row
            }


            // loop through rows
            for (int rowNum = rowStart; rowNum <= range.End.Row; rowNum++)
            {
                DataRow row = dt.NewRow();

                int dataTableColumnIndex = 0;// !!!! REMEMBER !!!! - The DataTable column indexes start at 0, the spreadsheet column index could be anything.  Understand that they are different...
                // loop through columns
                for (int columnNum = range.Start.Column; columnNum <= range.End.Column; columnNum++)
                {
                    object cellValue = ExcelReadUtility.ReadCell(range.Worksheet, rowNum, columnNum);
                    row[dataTableColumnIndex] = cellValue;
                    ++dataTableColumnIndex;
                }// end of looping through columns


                dt.Rows.Add(row);
            }// end of looping through rows


            return dt;
        }



    }
}
