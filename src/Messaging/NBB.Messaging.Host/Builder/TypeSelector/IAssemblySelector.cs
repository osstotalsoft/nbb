using System;
using System.Collections.Generic;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    /// <summary>
    /// Scans assemblies for message types
    /// </summary>
    public interface IAssemblySelector
    {
        /// <summary>
        /// Scans the assembly of T for message types.
        /// </summary>
        /// <typeparam name="T">A type in the assembly</typeparam>
        /// <returns>The implementation type selector</returns>
        IImplementationTypeSelector FromAssemblyOf<T>();

        /// <summary>
        /// Scans the calling assembly for message types.
        /// </summary>
        /// <returns>The implementation type selector</returns>
        IImplementationTypeSelector FromCallingAssembly();

        /// <summary>
        /// Scans the executing assembly for message types
        /// </summary>
        /// <returns>The implementation type selector</returns>
        IImplementationTypeSelector FromExecutingAssembly();

        /// <summary>
        /// Scans the entry assembly for message types.
        /// </summary>
        /// <returns>The implementation type selector</returns>
        IImplementationTypeSelector FromEntryAssembly();

        /// <summary>
        /// Scans the assemblies of the specified message types.
        /// </summary>
        /// <param name="types">Types defined in the target assemblies.</param>
        /// <returns>The implementation type selector</returns>
        IImplementationTypeSelector FromAssembliesOf(params Type[] types);

        /// <summary>
        /// Scans the assemblies of the specified message types.
        /// </summary>
        /// <param name="types">Types defined in the target assemblies.</param>
        /// <returns>The implementation type selector</returns>
        IImplementationTypeSelector FromAssembliesOf(IEnumerable<Type> types);

        /// <summary>
        /// Scans the given assemblies for message types.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>The implementation type selector</returns>
        IImplementationTypeSelector FromAssemblies(params Assembly[] assemblies);

        /// <summary>
        /// Scans the given assemblies for message types.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>The implementation type selector</returns>
        IImplementationTypeSelector FromAssemblies(IEnumerable<Assembly> assemblies);
    }
}
