using System.Collections.Generic;
using System.IO;

namespace NBB.Exporter
{
    /// <summary>
    /// Used to export a List<typeparam name="T">to any needed format</typeparam>
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public interface IAbstractDataExport<T>
    {
        /// <summary>
        /// Performs the export
        /// </summary>
        /// <param name="exportData">The list</param>
        /// <param name="properties">Optional properties needed by some exporters</param>
        /// <param name="headers">Optional headers for Excel export</param>
        /// <returns>File stream</returns>
        Stream Export(List<T> exportData, Dictionary<string, string> properties = null, List<string> headers = null);
    }
}