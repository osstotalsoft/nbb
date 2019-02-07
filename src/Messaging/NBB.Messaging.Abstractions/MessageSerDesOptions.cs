using System.Collections.Generic;
using System.Reflection;

namespace NBB.Messaging.Abstractions
{
    /// <summary>
    /// Options for serialization/deserialization
    /// </summary>
    public class MessageSerDesOptions
    {
        /// <summary>
        ///  Options for message deserialization.
        /// </summary>
        /// <value>
        /// The type of deserialization.
        /// </value>
        public DeserializationType DeserializationType { get; set; } = DeserializationType.TypeSafe;
        /// <summary>
        /// he list of assemblies to be scanned in case of the 'Dynamic' deserialization option.
        /// </summary>
        /// <value>
        /// The scanned assemblies used in dynamic deserialization.
        /// </value>
        public IEnumerable<Assembly> DynamicDeserializationScannedAssemblies { get; set; }
    }

    /// <summary>
    /// Used to configure the serialization/deserialization options
    /// </summary>
    public class MessageSerDesOptionsBuilder
    {
        /// <summary>
        /// Gets the serialization/deserialization options that are currently being configured.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public MessageSerDesOptions Options { get; }

        public MessageSerDesOptionsBuilder(MessageSerDesOptions options = null)
        {
            Options = new MessageSerDesOptions();

            if (options != null)
            {
                Options.DeserializationType = options.DeserializationType;
                Options.DynamicDeserializationScannedAssemblies = options.DynamicDeserializationScannedAssemblies;
            }
        }
    }

    public enum DeserializationType
    {
        /// <summary>
        /// Uses strongly typed deserialization if possible
        /// </summary>
        TypeSafe = 0,
        /// <summary>
        /// Uses type information included in the MessageType header to deserialize the message
        /// </summary>
        Dynamic = 1,
        /// <summary>
        /// Deserialize the headers but keep payload serialized
        /// </summary>
        HeadersOnly = 2
    }
}
