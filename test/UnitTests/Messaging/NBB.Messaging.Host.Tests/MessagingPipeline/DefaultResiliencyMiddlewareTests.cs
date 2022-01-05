// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NBB.Messaging.Abstractions;
using Xunit;

namespace NBB.Messaging.Host.Tests.MessagingPipeline
{
    public class DefaultResiliencyMiddlewareTests
    {
        [Fact]
        public async void Should_callNextPipelineMiddleware()
        {
            //Arrange
            var resiliencyMiddleware = new DefaultResiliencyMiddleware(
                Mock.Of<ILogger<DefaultResiliencyMiddleware>>());

            var sentMessage = new { Field = "value"};
            var isNextMiddlewareCalled = false;
            var envelope = new MessagingEnvelope(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() { isNextMiddlewareCalled = true; return Task.CompletedTask; }

            //Act
            await resiliencyMiddleware.Invoke(new MessagingContext(envelope, string.Empty, null),default, Next);

            //Assert
            isNextMiddlewareCalled.Should().BeTrue();
        }

     
        [Fact]
        public async Task Should_throwGenericException()
        {
            //Arrange
            var mockedLogger = Mock.Of<ILogger<DefaultResiliencyMiddleware>>();
            var resiliencyMiddleware = new DefaultResiliencyMiddleware(
                mockedLogger);

            var sentMessage = new { Field = "value"};
            var envelope = new MessagingEnvelope(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() => throw new ApplicationException();

            //Act
            async Task Action()
            {
                await resiliencyMiddleware.Invoke(new MessagingContext(envelope, string.Empty, null), default, Next);
            }

            //Assert
            await ((Func<Task>) Action).Should().ThrowAsync<ApplicationException>();
        }

        [Fact]
        public async Task Should_throwPolicyException()
        {
            //Arrange
            var mockedLogger = Mock.Of<ILogger<DefaultResiliencyMiddleware>>();
            var resiliencyMiddleware = new DefaultResiliencyMiddleware(
                mockedLogger);

            var sentMessage = new { Field = "value"};
            var envelope = new MessagingEnvelope(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() => throw new ApplicationException();

            //Act
            async Task Action()
            {
                await resiliencyMiddleware.Invoke(new MessagingContext(envelope, string.Empty, null), default, Next);
            }

            //Assert
            await ((Func<Task>) Action).Should().ThrowAsync<ApplicationException>();
        }
    }
}
