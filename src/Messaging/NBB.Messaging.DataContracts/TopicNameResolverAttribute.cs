// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using Microsoft.Extensions.Configuration;

namespace NBB.Messaging.DataContracts
{
    public abstract class TopicNameResolverAttribute : Attribute
    {
        public abstract string ResolveTopicName(Type messageType, IConfiguration configuration);
    }
}
