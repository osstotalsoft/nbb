// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;

namespace NBB.Core.Configuration.DotEnv
{
    /// <summary>
    /// An DotEnv file based <see cref="ConfigurationProvider"/>.
    /// Files are simple line structures with environment variable declarations as key-value pairs
    /// </summary>
    /// <examples>
    /// key1=value1
    /// key2 = " value2 "
    /// # comment
    /// </examples>
    public class DotEnvConfigurationProvider : FileConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance with the specified source.
        /// </summary>
        /// <param name="source">The source settings.</param>
        public DotEnvConfigurationProvider(DotEnvConfigurationSource source) : base(source) { }

        /// <summary>
        /// Loads the DotEnv data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        public override void Load(Stream stream)
            => Data = Read(stream);

        /// <summary>
        /// Read a stream of DotEnv values into a key/value dictionary.
        /// </summary>
        /// <param name="stream">The stream of DotEnv data.</param>
        /// <returns>The <see cref="IDictionary{String, String}"/> which was read from the stream.</returns>
        private static IDictionary<string, string?> Read(Stream stream)
        {
            var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            using (var reader = new StreamReader(stream))
            {
                string sectionPrefix = string.Empty;

                while (reader.Peek() != -1)
                {
                    string rawLine = reader.ReadLine()!;
                    string line = rawLine.Trim();

                    // Ignore blank lines
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    // Ignore comments
                    if (line[0] is '#')
                    {
                        continue;
                    }

                    // key = value OR "value"
                    int separator = line.IndexOf('=');
                    if (separator < 0)
                    {
                        throw new FormatException($"Unrecognized line formmat: {rawLine}");
                    }

                    string key = sectionPrefix + line.Substring(0, separator).Trim();
                    string value = line.Substring(separator + 1).Trim();

                    // Remove quotes
                    if (value.Length > 1 && value[0] == '"' && value[value.Length - 1] == '"')
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    if (data.ContainsKey(key))
                    {
                        throw new FormatException($"Duplicated key: {key}");
                    }

                    var normalizedKey = Normalize(key);
                    data[normalizedKey] = value;
                }
            }
            return data;
        }

        private static string Normalize(string key)
            => key.Replace("__", ConfigurationPath.KeyDelimiter);
    }
}
