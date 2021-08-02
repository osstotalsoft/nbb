using System;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    /// <summary>
    /// Adds implementation classes (non-abstract) from the current message type source
    /// to the configuration used to create subscriber services
    /// </summary>
    public interface IImplementationTypeSelector
    {
        /// <summary>
        /// Add the classes assignable to type <typeparamref name="TBase"/> to the configuration used to create subscriber services.
        /// Only non-abstract derived classes/ implementations from the current source are selected.
        /// </summary>
        /// <typeparam name="TBase">The type of the base class/ interface.</typeparam>
        /// <param name="publicOnly">if set to <c>true</c> only public types are selected.</param>
        /// <returns>Next type source selector. It is used in the fluent API.</returns>
        ITypeSourceSelector AddClassesAssignableTo<TBase>(bool publicOnly = true);

        /// <summary>
        /// Add classes that satisfy the conditions to the configuration used to create subscriber services.
        /// Only non-abstract classes from the current source are selected.
        /// </summary>
        /// <param name="predicate">The predicate for specifying the conditions.</param>
        /// <param name="publicOnly">if set to <c>true</c> only public types are selected.</param>
        /// <returns>Next type source selector. It is used in the fluent API.</returns>
        ITypeSourceSelector AddClassesWhere(Func<Type, bool> predicate, bool publicOnly = true);

        /// <summary>
        /// Add all classes to the configuration used to create subscriber services.
        /// Only non-abstract classes from the current source are selected.
        /// </summary>
        /// <param name="publicOnly">if set to <c>true</c> only public types are selected.</param>
        /// <returns>Next type source selector. It is used in the fluent API.</returns>
        ITypeSourceSelector AddAllClasses(bool publicOnly = true);
    }
}
