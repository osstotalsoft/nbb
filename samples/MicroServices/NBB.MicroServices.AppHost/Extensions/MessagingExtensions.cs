// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

internal static class MessagingExtensions
{
    /// <summary>
    /// Points the NBB JetStream transport of a service at the given NATS server.
    /// </summary>
    internal static IResourceBuilder<T> WithJetStream<T>(this IResourceBuilder<T> resource, IResourceBuilder<IResourceWithConnectionString> jetstream)
        where T : IResourceWithEnvironment =>
        resource.WithEnvironment("Messaging__JetStream__NatsUrl", jetstream);
}
