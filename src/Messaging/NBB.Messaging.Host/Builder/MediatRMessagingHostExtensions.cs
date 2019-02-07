using MediatR;
using NBB.Messaging.Host.Builder.TypeSelector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NBB.Messaging.Host.Builder
{
    /// <summary>
    /// Extend the <seealso cref="NBB.Messaging.Host.Builder.TypeSelector.ITypeSourceSelector" />
    /// with methods for selecting message types from MediatR IoC handler registrations.
    /// </summary>
    public static class MediatorMessagingHostBuilderExtensions
    {
        private static readonly Type eventType = typeof(INotificationHandler<>);
        private static readonly Type commandType = typeof(IRequestHandler<>);
        private static readonly Type queryType = typeof(IRequestHandler<,>);

        /// <summary>
        /// Scans the the MediatR IoC registrations for handled messages types (commands, queries and events).
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatRHandledMessages(
            this ITypeSourceSelector typeSourceSelector)
            => FromMediatRHandledMessagesInternal(typeSourceSelector, new[] {eventType, commandType, queryType});

        /// <summary>
        /// Scans the the MediatR IoC registrations for handled events.
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatRHandledEvents(this ITypeSourceSelector typeSourceSelector)
            => FromMediatRHandledMessagesInternal(typeSourceSelector, new[] {eventType});
        
        /// <summary>
        /// Scans the the MediatR IoC registrations for handled commands.
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatRHandledCommands(this ITypeSourceSelector typeSourceSelector)
            => FromMediatRHandledMessagesInternal(typeSourceSelector, new[] {commandType});

        /// <summary>
        /// Scans the the MediatR IoC registrations for handled queries.
        /// </summary>
        /// <param name="typeSourceSelector">The type source selector.</param>
        /// <returns></returns>
        public static IImplementationTypeSelector FromMediatRHandledQueries(this ITypeSourceSelector typeSourceSelector)
            => FromMediatRHandledMessagesInternal(typeSourceSelector, new[] {queryType});

        private static IImplementationTypeSelector FromMediatRHandledMessagesInternal(
            ITypeSourceSelector typeSourceSelector, IEnumerable<Type> handlerTypes)
        {
           
            var handlers = ((IServiceCollectionProvider)typeSourceSelector).ServiceCollection
                .Select(sd => sd.ServiceType)
                .Where(t => t.IsGenericType && handlerTypes.Contains(t.GetGenericTypeDefinition()));

            var integrationEventTypes = handlers
                .Select(t => t.GetGenericArguments()[0]).ToList();

            var selector = new ImplementationTypeSelector(typeSourceSelector, integrationEventTypes);
            ((IMessageTypeProvider)typeSourceSelector).RegisterTypes(selector);

            return selector;
        }
    }
}
