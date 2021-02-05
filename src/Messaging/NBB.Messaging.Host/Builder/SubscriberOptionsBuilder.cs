using NBB.Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NBB.Messaging.Host.Builder
{
    /// <summary>
    /// Used to configure subscriber options
    /// </summary>
    public class SubscriberOptionsBuilder
    {
        private MessagingSubscriberOptions _subscriberOptions = new();

        /// <summary>
        /// Configures messaging transport options
        /// </summary>
        /// <param name="subscriberOptionsConfigurator"></param>
        /// <returns></returns>
        public SubscriberOptionsBuilder ConfigureTransport(
            Func<SubscriptionTransportOptions, SubscriptionTransportOptions> subscriberOptionsConfigurator)
        {
            _subscriberOptions = _subscriberOptions with
            {
                Transport = subscriberOptionsConfigurator(_subscriberOptions.Transport)
            };

            return this;
        }

        /// <summary>
        /// Adds support for dynamic deserialization (based on the MessageType received in the headers)
        /// </summary>
        /// <param name="dynamicDeserializationScannedAssemblies">The assemblies where the message type can ve found</param>
        /// <returns></returns>
        public SubscriberOptionsBuilder UseDynamicDeserialization(
            IEnumerable<Assembly> dynamicDeserializationScannedAssemblies)
        {
            _subscriberOptions = _subscriberOptions with
            {
                SerDes = _subscriberOptions.SerDes with
                {
                    UseDynamicDeserialization = true,
                    DynamicDeserializationScannedAssemblies = dynamicDeserializationScannedAssemblies
                }
            };

            return this;
        }

        internal MessagingSubscriberOptions Build()
            => _subscriberOptions;
    }
}