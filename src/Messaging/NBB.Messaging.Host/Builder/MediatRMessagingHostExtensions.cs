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

        private static readonly TypeInfo eventType = new TypeInfo(typeof(INotificationHandler<>), _ => true);
        private static readonly TypeInfo commandType = new TypeInfo(typeof(IRequestHandler<,>), types => types[1] == typeof(Unit));
        private static readonly TypeInfo queryType = new TypeInfo(typeof(IRequestHandler<,>), types => types[1] != typeof(Unit));

        /// <summary>
        /// Scans the the MediatR IoC registrations for handled messages types (commands, queries and events).
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatRHandledMessages(
            this ITypeSourceSelector typeSourceSelector)
            => FromMediatRHandledMessagesInternal(typeSourceSelector, new[] { eventType, commandType, queryType });

        /// <summary>
        /// Scans the the MediatR IoC registrations for handled events.
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatRHandledEvents(this ITypeSourceSelector typeSourceSelector)
            => FromMediatRHandledMessagesInternal(typeSourceSelector, new[] { eventType });

        /// <summary>
        /// Scans the the MediatR IoC registrations for handled commands.
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatRHandledCommands(this ITypeSourceSelector typeSourceSelector)
            => FromMediatRHandledMessagesInternal(typeSourceSelector, new[] { commandType });

        /// <summary>
        /// Scans the the MediatR IoC registrations for handled queries.
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatRHandledQueries(this ITypeSourceSelector typeSourceSelector)
            => FromMediatRHandledMessagesInternal(typeSourceSelector, new[] { queryType });

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
