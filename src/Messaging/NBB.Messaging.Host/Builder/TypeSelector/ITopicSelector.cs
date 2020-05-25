using System.Collections.Generic;

namespace NBB.Messaging.Host.Builder.TypeSelector
{
    /// <summary>
    /// Selects message topics for creating subscriber services
    /// </summary>
    public interface ITopicSelector
    {
        /// <summary>
        /// Includes the specified topic in the configuration used to create subscriber services.
        /// </summary>
        /// <param name="topic">The topic name.</param>
        /// <returns>The next selector for sources of message types</returns>
        ITypeSourceSelector FromTopic(string topic);

        /// <summary>
        /// Includes the specified topic(s) in the configuration used to create subscriber services.
        /// </summary>
        /// <param name="topics">The topic name(s).</param>
        /// <returns>The next selector for sources of message types</returns>
        ITypeSourceSelector FromTopics(params string[] topics);

        /// <summary>
        /// Includes the specified topics in the configuration used to create subscriber services.
        /// </summary>
        /// <param name="topics">The topic names.</param>
        /// <returns>The next selector for sources of message types</returns>
        ITypeSourceSelector FromTopics(IEnumerable<string> topics);
    }
}
