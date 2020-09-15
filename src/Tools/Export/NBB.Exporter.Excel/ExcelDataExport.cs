using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NBB.Exporter.Excel
{
    /// <summary>
    /// Excel export implementation of IAbstractDataExport
    /// Uses reflection to get the properties of type T
    /// </summary>
    /// <typeparam name="T">Type to export</typeparam>
    public class ExcelDataExport<T> : IAbstractDataExport<T> where T : class
    {
        private const string DefaultSheetName = "Sheet1";

        /// <summary>
        /// Exports a list of objects of a type as excel sheet.
        /// </summary>
        /// <param name="exportData">The data to be exported.</param>
        /// <param name="properties">Additional Excel sheet properties.</param>
        /// <param name="headers">The header name for each column.</param>
        /// <returns></returns>
        public Stream Export(List<T> exportData, Dictionary<string, string> properties = null, List<string> headers = null)
        {
            if (exportData == null)
                throw new ArgumentNullException(nameof(exportData));

            var types = new List<string>();
            var excelHeaders = headers ?? new List<string>();
            var hasHeaders = excelHeaders.Any();

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                types.Add(type.Name);

                //name by caps for header
                if (hasHeaders) continue;

                var name = Regex.Replace(prop.Name, "([A-Z])", " $1").Trim(); //space separated 
                excelHeaders.Add(name);
            }


            var exportDataList = new List<List<object>>();

            for (var i = 0; i < exportData.Count; i++)
            {
                var exportDataItem = new List<object>();
                var item = exportData[i];
                for (var j = 0; j < excelHeaders.Count; j++)
                {
                    var prop = props[j];
                    var currentCellValue = prop.GetValue(item) ?? DBNull.Value;
                    exportDataItem.Add(currentCellValue);
                }

                exportDataList.Add(exportDataItem);
            }

            return ExportFromListOfObjects(exportDataList, types, excelHeaders, properties);
        }

        /// <summary>
        /// Exports a list of basic objects as excel sheet.
        /// </summary>
        /// <param name="exportData">The data to be exported.</param>
        /// <param name="columnTypes">The types for each column. Supported types: string, int, double, datetime.</param>
        /// <param name="headers">The header name for each column.</param>
        /// <param name="properties">Additional Excel sheet properties.</param>
        /// <returns></returns>
        public Stream ExportFromListOfObjects(List<List<object>> exportData, List<string> columnTypes, List<string> headers, Dictionary<string, string> properties = null)
        {
            if (exportData == null)
                throw new ArgumentNullException("exportData");

            if (columnTypes == null)
                throw new ArgumentNullException("columnTypes");

            if (headers == null)
                throw new ArgumentNullException("headers");

            var sheetName = properties != null && properties.ContainsKey("SheetName") ? properties["SheetName"] : DefaultSheetName;

            var workbook = new XSSFWorkbook(); //Creating New Excel object
            var sheet = workbook.CreateSheet(sheetName); //Creating New Excel Sheet object

            var headerStyle = workbook.CreateCellStyle(); //Formatting
            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);

            //Header
            var header = sheet.CreateRow(0);
            for (var i = 0; i < headers.Count; i++)
            {
                var cell = header.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            for (var i = 0; i < exportData.Count; i++)
            {
                var sheetRow = sheet.CreateRow(i + 1);
                var item = exportData[i];

                for (var j = 0; j < headers.Count; j++)
                {
                    var row = sheetRow.CreateCell(j);
                    var prop = item[j];

                    var type = columnTypes[j].ToLower();

                    if (prop != null &&
                        !string.IsNullOrEmpty(Convert.ToString(prop)))
                    {
                        switch (type)
                        {
                            case "string":
                                row.SetCellValue(Convert.ToString(prop));
                                break;
                            case "int32":
                                row.SetCellValue(Convert.ToInt32(prop));
                                break;
                            case "double":
                                row.SetCellValue(Convert.ToDouble(prop));
                                break;
                            case "datetime":
                                row.SetCellValue(Convert.ToDateTime(prop));
                                break;
                            default:
                                row.SetCellValue(Convert.ToString(prop));
                                break;
                        }
                    }
                    else
                    {
                        row.SetCellValue(string.Empty);
                    }
                }
            }

            using (var memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream, true);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var result = new MemoryStream();
                memoryStream.CopyTo(result);
                result.Seek(0, SeekOrigin.Begin);
                return result;
            }
        }
    }
}