using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NBB.Core.Abstractions;
using NBB.Messaging.DataContracts;
using NBB.Messaging.Host.MessagingPipeline;
using NBB.Resiliency;
using Polly;
using Xunit;

namespace NBB.Messaging.Host.Tests.MessagingPipeline
{
    public class DefaultResiliencyMiddlewareTests
    {
        [Fact]
        public async void Should_callNextPipelineMiddleware()
        {
            //Arrange
            var dummyPolicy = Policy.Handle<Exception>().RetryAsync(0);

            var resiliencyMiddleware = new DefaultResiliencyMiddleware(
                Mock.Of<IResiliencyPolicyProvider>(x => 
                    x.GetConcurencyExceptionPolicy(It.IsAny<Action<Exception>>()) == dummyPolicy &&
                    x.GetOutOfOrderPolicy(It.IsAny<Action<int>>()) == dummyPolicy), 
                Mock.Of<ILogger<DefaultResiliencyMiddleware>>());

            var sentMessage = Mock.Of<IMessage>();
            var isNextMiddlewareCalled = false;
            var envelope = new MessagingEnvelope<IMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() { isNextMiddlewareCalled = true; return Task.CompletedTask; }

            //Act
            await resiliencyMiddleware.Invoke(envelope, default(CancellationToken), Next);

            //Assert
            isNextMiddlewareCalled.Should().BeTrue();
        }

        [Fact]
        public async void Should_callOutOfOrderResiliencyPolicy()
        {
            //Arrange
            var dummyPolicy = Policy.Handle<Exception>().RetryAsync(0);
            var outOfOrderPoliyCalled = false;
            var outOfOrderPolicy = Policy.Handle<OutOfOrderMessageException>()
                .FallbackAsync(ct =>
                {
                    outOfOrderPoliyCalled = true;
                    return Task.CompletedTask;
                });

            var resiliencyMiddleware = new DefaultResiliencyMiddleware(
                Mock.Of<IResiliencyPolicyProvider>(x =>
                    x.GetConcurencyExceptionPolicy(It.IsAny<Action<Exception>>()) == dummyPolicy &&
                    x.GetOutOfOrderPolicy(It.IsAny<Action<int>>()) == outOfOrderPolicy),
                Mock.Of<ILogger<DefaultResiliencyMiddleware>>());

            var sentMessage = Mock.Of<IMessage>();
            var envelope = new MessagingEnvelope<IMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() => throw new OutOfOrderMessageException();

            //Act
            await resiliencyMiddleware.Invoke(envelope, default(CancellationToken), Next);
            //Assert
            outOfOrderPoliyCalled.Should().BeTrue();
        }

        [Fact]
        public async void Should_callConcurrencyResiliencyPolicy()
        {
            //Arrange
            var dummyPolicy = Policy.Handle<Exception>().RetryAsync(0);
            var concurrencyPoliyCalled = false;
            var concurrencyPolicy = Policy.Handle<ConcurrencyException>()
                .FallbackAsync(ct =>
                {
                    concurrencyPoliyCalled = true;
                    return Task.CompletedTask;
                });

            var resiliencyMiddleware = new DefaultResiliencyMiddleware(
                Mock.Of<IResiliencyPolicyProvider>(x =>
                    x.GetConcurencyExceptionPolicy(It.IsAny<Action<Exception>>()) == concurrencyPolicy &&
                    x.GetOutOfOrderPolicy(It.IsAny<Action<int>>()) == dummyPolicy),
                Mock.Of<ILogger<DefaultResiliencyMiddleware>>());

            var sentMessage = Mock.Of<IMessage>();
            var envelope = new MessagingEnvelope<IMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() => throw new ConcurrencyException("message");

            //Act
            await resiliencyMiddleware.Invoke(envelope, default(CancellationToken), Next);
            //Assert
            concurrencyPoliyCalled.Should().BeTrue();
        }

        [Fact]
        public void Should_throwGenericException()
        {
            //Arrange
            var dummyPolicy = Policy.Handle<ArgumentException>().RetryAsync(0);
            var mockedLogger = Mock.Of<ILogger<DefaultResiliencyMiddleware>>();
            var resiliencyMiddleware = new DefaultResiliencyMiddleware(
                Mock.Of<IResiliencyPolicyProvider>(x =>
                    x.GetConcurencyExceptionPolicy(It.IsAny<Action<Exception>>()) == dummyPolicy &&
                    x.GetOutOfOrderPolicy(It.IsAny<Action<int>>()) == dummyPolicy),
                mockedLogger);

            var sentMessage = Mock.Of<IMessage>();
            var envelope = new MessagingEnvelope<IMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() => throw new ApplicationException();

            //Act
            async Task Action()
            {
                await resiliencyMiddleware.Invoke(envelope, default(CancellationToken), Next);
            }

            //Assert
            ((Func<Task>) Action).Should().Throw<ApplicationException>();
        }

        [Fact]
        public void Should_throwPolicyException()
        {
            //Arrange
            var dummyPolicy = Policy.Handle<ApplicationException>().RetryAsync(1);
            var mockedLogger = Mock.Of<ILogger<DefaultResiliencyMiddleware>>();
            var resiliencyMiddleware = new DefaultResiliencyMiddleware(
                Mock.Of<IResiliencyPolicyProvider>(x =>
                    x.GetConcurencyExceptionPolicy(It.IsAny<Action<Exception>>()) == dummyPolicy &&
                    x.GetOutOfOrderPolicy(It.IsAny<Action<int>>()) == dummyPolicy),
                mockedLogger);

            var sentMessage = Mock.Of<IMessage>();
            var envelope = new MessagingEnvelope<IMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() => throw new ApplicationException();

            //Act
            async Task Action()
            {
                await resiliencyMiddleware.Invoke(envelope, default(CancellationToken), Next);
            }

            //Assert
            ((Func<Task>) Action).Should().Throw<ApplicationException>();
        }
    }
}
