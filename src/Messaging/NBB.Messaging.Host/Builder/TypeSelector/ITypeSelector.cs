using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    /// <summary>
    /// Adds the specified types
    /// </summary>
    public interface ITypeSelector
    {
        /// <summary>
        /// Add the type <typeparamref name="TMessage"/> to the configuration used to create subscriber services.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <returns>Next type source selector. It is used in the fluent API.</returns>
        ITypeSourceSelector AddType<TMessage>();

        /// <summary>
        /// Add the specified type(s) to the configuration used to create subscriber services.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <returns>Next type source selector. It is used in the fluent API.</returns>
        ITypeSourceSelector AddTypes(params Type[] types);

        /// <summary>
        /// Add the specified types to the configuration used to create subscriber services.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <returns>Next type source selector. It is used in the fluent API.</returns>
        ITypeSourceSelector AddTypes(IEnumerable<Type> types);
    }
}
