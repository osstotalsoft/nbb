// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;

namespace NBB.Core.Configuration.DotEnv
{
    /// <summary>
    /// Represents a DotEnv file as an <see cref="IConfigurationSource"/>.
    /// Files are simple line structures with environment variable declarations as key-value pairs
    /// </summary>
    /// <examples>
    /// key1=value1
    /// key2 = " value2 "
    /// # comment
    /// </examples>
    public class DotEnvConfigurationSource : FileConfigurationSource
    {
        /// <summary>
        /// Builds the <see cref="IniConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>An <see cref="IniConfigurationProvider"/></returns>
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new DotEnvConfigurationProvider(this);
        }
    }
}
