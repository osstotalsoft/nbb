// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using System.Reflection;

namespace NBB.Messaging.Abstractions
{
    /// <summary>
    /// Options for serialization/deserialization
    /// </summary>
    public record MessageSerDesOptions
    {
        /// <summary>
        ///  Options for message deserialization.
        /// </summary>
        /// <value>
        /// The type of deserialization.
        /// </value>
        public bool UseDynamicDeserialization { get; init; }
        /// <summary>
        /// he list of assemblies to be scanned in case of the 'Dynamic' deserialization option.
        /// </summary>
        /// <value>
        /// The scanned assemblies used in dynamic deserialization.
        /// </value>
        public IEnumerable<Assembly> DynamicDeserializationScannedAssemblies { get; init; }

        public static MessageSerDesOptions Default { get; } = new();
    }
    
}
