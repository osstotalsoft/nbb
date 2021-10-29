// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    /// <summary>
    /// Extend the <seealso cref="NBB.Messaging.Host.Builder.TypeSelector.ITypeSourceSelector" />
    /// with methods for selecting message types from MediatR IoC handler registrations.
    /// </summary>
    public static class MediatorMessagingHostBuilderExtensions
    {
        private record TypeInfo(Type GenericTypeDef, Func<Type[], bool> Condition);

        private static readonly TypeInfo EventType = new(typeof(INotificationHandler<>), _ => true);
        private static readonly TypeInfo CommandType = new(typeof(IRequestHandler<,>), types => types[1] == typeof(Unit));
        private static readonly TypeInfo QueryType = new(typeof(IRequestHandler<,>), types => types[1] != typeof(Unit));

        /// <summary>
        /// Scans the the MediatR IoC registrations for handled messages types (commands, queries and events).
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatRHandledMessages(
            this ITypeSourceSelector typeSourceSelector)
            => FromMediatRHandledMessagesInternal(typeSourceSelector, new[] { EventType, CommandType, QueryType });

        /// <summary>
        /// Scans the the MediatR IoC registrations for handled events.
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatRHandledEvents(this ITypeSourceSelector typeSourceSelector)
            => FromMediatRHandledMessagesInternal(typeSourceSelector, new[] { EventType });

        /// <summary>
        /// Scans the the MediatR IoC registrations for handled commands.
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatRHandledCommands(this ITypeSourceSelector typeSourceSelector)
            => FromMediatRHandledMessagesInternal(typeSourceSelector, new[] { CommandType });

        /// <summary>
        /// Scans the the MediatR IoC registrations for handled queries.
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatRHandledQueries(this ITypeSourceSelector typeSourceSelector)
            => FromMediatRHandledMessagesInternal(typeSourceSelector, new[] { QueryType });

        private static IImplementationTypeSelector FromMediatRHandledMessagesInternal(
            ITypeSourceSelector typeSourceSelector, IEnumerable<TypeInfo> handlerTypes)
        {
            var handlers = ((IServiceCollectionProvider)typeSourceSelector).ServiceCollection
                .Select(sd => sd.ServiceType)
                .Where(t =>
                    t.IsGenericType &&
                    handlerTypes.Any(typeInfo =>
                        typeInfo.GenericTypeDef == t.GetGenericTypeDefinition() &&
                        typeInfo.Condition.Invoke(t.GetGenericArguments())));

            var integrationEventTypes = handlers
                .Select(t => t.GetGenericArguments()[0]).ToList();

            var selector = new ImplementationTypeSelector(typeSourceSelector, integrationEventTypes);
            ((IMessageTypeProvider)typeSourceSelector).RegisterTypes(selector);

            return selector;
        }
    }
}
