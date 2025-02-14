// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Options;
using System;

namespace NBB.Messaging.Abstractions;
public record MessagingOptions : IOptions<MessagingOptions>
{
    /// <summary>
    /// The code of environment in which the application is running. It will be used as a prefix for the topics.
    /// </summary>
    public string Env { get; init; }

    /// <summary>
    /// The name of the source of the messages. It will be added as a header to messages.
    /// </summary>
    public string Source { get; init; }

    [Obsolete("Ensures backward compatibility. Use Env instead.")]
    public string TopicPrefix { get; init; }

    public MessagingOptions Value => this;
}
