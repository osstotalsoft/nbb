// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    public interface IServiceCollectionProvider
    {
        IServiceCollection ServiceCollection { get; }
    }
}
