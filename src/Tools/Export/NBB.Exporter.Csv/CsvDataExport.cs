using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace NBB.Exporter.Csv
{
    public class CsvDataExport<T> : IAbstractDataExport<T> where T : class
    {
        /// <summary>
        /// Exports a list of objects as csv.
        /// </summary>
        /// <param name="exportData">The data to be exported.</param>
        /// <param name="headers">The header name for each column.</param>
        /// <param name="properties">Additional Excel sheet properties.</param>
        /// <returns></returns>
        public Stream Export(List<T> exportData, Dictionary<string, string> properties = null, List<string> headers = null)
        {
            if (exportData == null)
                throw new ArgumentNullException("exportData");

            var newLine = Environment.NewLine;
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var exportDataList = new List<List<object>>();

            headers = headers ?? props.Select(x => Regex.Replace(x.Name, "([A-Z])", " $1").Trim()).ToList();
            for (var i = 0; i < exportData.Count; i++)
            {
                var exportDataItem = new List<object>();
                var item = exportData[i];
                for (var j = 0; j < headers.Count; j++)
                {
                    var prop = props[j];
                    var value = prop.GetValue(item) ?? string.Empty;
                    exportDataItem.Add(value);
                }

                exportDataList.Add(exportDataItem);
            }

            return ExportFromListOfObjects(exportDataList, headers, properties);
        }

        /// <summary>
        /// Exports a list of basic objects as csv.
        /// </summary>
        /// <param name="exportData">The data to be exported.</param>
        /// <param name="headers">The header name for each column.</param>
        /// <param name="properties">Additional Excel sheet properties.</param>
        /// <returns></returns>
        public Stream ExportFromListOfObjects(List<List<object>> exportData, List<string> headers, Dictionary<string, string> properties = null)
        {
            if (exportData == null)
                throw new ArgumentNullException("exportData");

            if (headers == null)
                throw new ArgumentNullException("headers");

            var newLine = Environment.NewLine;
            var separator = ",";
            if (properties != null && properties.ContainsKey("Separator"))
                separator = properties["Separator"];
            
            string csvHeaderRow = string.Join(separator, headers) + newLine;
            var csvRows = new StringBuilder();

            foreach (var lineData in exportData)
            {
                csvRows.AppendJoin(separator, lineData).Append(newLine);
            }

            var csv = csvHeaderRow + csvRows;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(csv);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}