﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using NBB.MultiTenancy.Identification.Resolvers;
using Xunit;

namespace NBB.MultiTenancy.Identification.Messaging.Tests
{
    public class TenantIdHeaderMessagingTokenResolverTests
    {
        private readonly MessagingContextAccessor _mockMessagingContextAccessor;
        private readonly Dictionary<string, string> _headers;

        public TenantIdHeaderMessagingTokenResolverTests()
        {
            _mockMessagingContextAccessor = new MessagingContextAccessor();
            _headers = new Dictionary<string, string>();
            var mockMessagingEnvelope = new MessagingEnvelope(_headers, new object());
            var mockMessagingContext = new MessagingContext(mockMessagingEnvelope);

            _mockMessagingContextAccessor.MessagingContext = mockMessagingContext;
        }

        [Fact]
        public void Should_Resolve_Token_FromHeader()
        {
            // Arrange
            const string key = "test token key";
            const string value = "test token value";
            _headers.Add(key, value);
            var sut = new TenantIdHeaderMessagingTokenResolver(_mockMessagingContextAccessor, key);

            // Act
            var result = sut.GetTenantToken().Result;

            // Assert
            result.Should().Be(value);
        }

        [Fact]
        public void Should_Throw_CannotResolveTokenException_For_Bad_Keys()
        {
            // Arrange
            const string key = "bad token key";
            var sut = new TenantIdHeaderMessagingTokenResolver(_mockMessagingContextAccessor, key);

            // Act
            Action act = () => sut.GetTenantToken();

            // Assert
            act.Should().Throw<CannotResolveTokenException>();
        }
    }
}
