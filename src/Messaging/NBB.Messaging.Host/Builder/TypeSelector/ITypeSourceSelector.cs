// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Messaging.Host
{
    /// <summary>
    /// Specifies the sources of message types/topics for creating subscribers
    /// </summary>
    /// <seealso cref="NBB.Messaging.Host.Builder.TypeSelector.IAssemblySelector" />
    /// <seealso cref="NBB.Messaging.Host.Builder.TypeSelector.ITypeSelector" />
    /// <seealso cref="NBB.Messaging.Host.Builder.TypeSelector.ITopicSelector" />
    public interface ITypeSourceSelector : IAssemblySelector, ITypeSelector, ITopicSelector
    {
    }
}
