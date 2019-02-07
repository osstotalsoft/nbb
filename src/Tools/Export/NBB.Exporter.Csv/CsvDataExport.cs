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
        public Stream Export(List<T> exportData, Dictionary<string, string> properties = null, List<string> headers = null)
        {
            if (exportData == null)
                throw new ArgumentNullException("exportData");

            var separator = ",";
            if (properties != null && properties.ContainsKey("Separator"))
                separator = properties["Separator"];
            
            var newLine = Environment.NewLine;
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            string csvHeaderRow;
            if (headers != null && headers.Any())
            {
                csvHeaderRow = string.Join(",", headers.ToArray<string>()) + newLine;
            }
            else
            {
                csvHeaderRow = string.Join(separator, props.Select(x => Regex.Replace(x.Name, "([A-Z])", " $1").Trim() ).ToArray()) + newLine;
            }

            var csvRows = new StringBuilder();
            var lastProp = props[props.Length - 1];

            foreach (var item in exportData)
            {
                for (var i = 0; i < props.Length - 1; i++)
                {
                    var prop = props[i];
                    csvRows.Append(prop.GetValue(item) + ",");
                }
                csvRows.Append(lastProp.GetValue(item) + newLine);
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