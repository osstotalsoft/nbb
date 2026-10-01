// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Mediator;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    /// <summary>
    /// Extend the <seealso cref="NBB.Messaging.Host.Builder.TypeSelector.ITypeSourceSelector" />
    /// with methods for selecting message types from Mediator IoC handler registrations.
    /// AddMediator() must be called before configuring the messaging host.
    /// </summary>
    public static class MediatorLibMessagingHostBuilderExtensions
    {
        private record TypeInfo(Type GenericTypeDef, Func<Type[], bool> Condition);

        private static bool IsUnit(Type[] types) => types[1] == typeof(Unit);

        private static readonly TypeInfo[] EventTypes = [new(typeof(INotificationHandler<>), _ => true)];

        private static readonly TypeInfo[] CommandTypes =
        [
            new(typeof(IRequestHandler<,>), IsUnit),
            new(typeof(ICommandHandler<,>), IsUnit)
        ];

        private static readonly TypeInfo[] QueryTypes =
        [
            new(typeof(IRequestHandler<,>), types => !IsUnit(types)),
            new(typeof(ICommandHandler<,>), types => !IsUnit(types)),
            new(typeof(IQueryHandler<,>), _ => true)
        ];

        /// <summary>
        /// Scans the Mediator IoC registrations for handled messages types (commands, queries and events).
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatorHandledMessages(this ITypeSourceSelector typeSourceSelector)
            => FromMediatorHandledMessagesInternal(typeSourceSelector, [.. EventTypes, .. CommandTypes, .. QueryTypes]);

        /// <summary>
        /// Scans the Mediator IoC registrations for handled events (notifications).
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatorHandledEvents(this ITypeSourceSelector typeSourceSelector)
            => FromMediatorHandledMessagesInternal(typeSourceSelector, EventTypes);

        /// <summary>
        /// Scans the Mediator IoC registrations for handled commands (requests and commands without a response).
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatorHandledCommands(this ITypeSourceSelector typeSourceSelector)
            => FromMediatorHandledMessagesInternal(typeSourceSelector, CommandTypes);

        /// <summary>
        /// Scans the Mediator IoC registrations for handled queries (queries, and requests and commands with a response).
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatorHandledQueries(this ITypeSourceSelector typeSourceSelector)
            => FromMediatorHandledMessagesInternal(typeSourceSelector, QueryTypes);

        private static IImplementationTypeSelector FromMediatorHandledMessagesInternal(
            ITypeSourceSelector typeSourceSelector, IEnumerable<TypeInfo> handlerTypes)
        {
            var handledMessageTypes = ((IServiceCollectionProvider)typeSourceSelector).ServiceCollection
                .Select(sd => sd.ServiceType)
                .Where(t =>
                    t.IsGenericType &&
                    handlerTypes.Any(typeInfo =>
                        typeInfo.GenericTypeDef == t.GetGenericTypeDefinition() &&
                        typeInfo.Condition.Invoke(t.GetGenericArguments())))
                .Select(t => t.GetGenericArguments()[0])
                .Distinct()
                .ToList();

            var selector = new ImplementationTypeSelector(typeSourceSelector, handledMessageTypes);
            ((IMessageTypeProvider)typeSourceSelector).RegisterTypes(selector);

            return selector;
        }
    }
}
