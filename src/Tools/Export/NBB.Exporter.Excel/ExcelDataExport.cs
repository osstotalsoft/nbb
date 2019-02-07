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

        public Stream Export(List<T> exportData, Dictionary<string, string> properties = null, List<string> headers = null)
        {
            if (exportData == null)
                throw new ArgumentNullException("exportData");

            var sheetName = properties != null && properties.ContainsKey("SheetName") ? properties["SheetName"] : DefaultSheetName;
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
            
            var workbook = new XSSFWorkbook(); //Creating New Excel object
            var sheet = workbook.CreateSheet(sheetName); //Creating New Excel Sheet object

            var headerStyle = workbook.CreateCellStyle(); //Formatting
            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);


            //Header
            var header = sheet.CreateRow(0);
            for (var i = 0; i < excelHeaders.Count; i++)
            {
                var cell = header.CreateCell(i);
                cell.SetCellValue(excelHeaders[i]);
                cell.CellStyle = headerStyle;
            }

            for (var i = 0; i < exportData.Count; i++)
            {
                var sheetRow = sheet.CreateRow(i + 1);
                var item = exportData[i];
                for (var j = 0; j < excelHeaders.Count; j++)
                {
                    var row = sheetRow.CreateCell(j);
                    var prop = props[j];

                    var type = types[j].ToLower();
                    var currentCellValue = prop.GetValue(item) ?? DBNull.Value;

                    if (currentCellValue != null &&
                        !string.IsNullOrEmpty(Convert.ToString(currentCellValue)))
                    {
                        switch (type)
                        {
                            case "string":
                                row.SetCellValue(Convert.ToString(currentCellValue));
                                break;
                            case "int32":
                                row.SetCellValue(Convert.ToInt32(currentCellValue));
                                break;
                            case "double":
                                row.SetCellValue(Convert.ToDouble(currentCellValue));
                                break;
                            case "datetime":
                                row.SetCellValue(Convert.ToDateTime(currentCellValue));
                                break;
                            default:
                                row.SetCellValue(Convert.ToString(currentCellValue));
                                break;
                        }
                    }
                    else
                    {
                        row.SetCellValue(string.Empty);
                    }
                }
            }

            // this will cause loading errors on linux. 
            // "Unable to load shared library 'libdl' or one of its dependencies.
            // In order to help diagnose loading problems, consider setting the LD_DEBUG environment variable:
            // liblibdl: cannot open shared object file
            //for (var i = 0; i < excelHeaders.Count; i++)
            //{
            //    sheet.AutoSizeColumn(i);
            //}

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