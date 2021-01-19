using System;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host.MessagingPipeline;
using Xunit;

namespace NBB.Messaging.Host.Tests.MessagingPipeline
{
    public class MediatRMiddlewareTests
    {
        [Fact]
        public async void Should_publishEventsToMediatR()
        {
            //Arrange
            var mockedMediator = Mock.Of<IMediator>();
            var mediatRMiddleware = new MediatRMiddleware(mockedMediator);
            var sentMessage = Mock.Of<IMockingEventMessage>();
            var envelope =
                new MessagingEnvelope<IMockingEventMessage>(new System.Collections.Generic.Dictionary<string, string>(),
                    sentMessage);

            Task Next() => Task.CompletedTask;

            //Act
            await mediatRMiddleware.Invoke(envelope, default, Next);

            //Assert
            Mock.Get(mockedMediator).Verify(x => x.Publish<INotification>(sentMessage, default), Times.Once);
        }

        [Fact]
        public async void Should_sendCommandsToMediatR()
        {
            //Arrange
            var mockedMediator = Mock.Of<IMediator>();
            var mediatRMiddleware = new MediatRMiddleware(mockedMediator);
            var sentMessage = Mock.Of<IMockingCommandMessage>();
            var envelope =
                new MessagingEnvelope<IMockingCommandMessage>(
                    new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() => Task.CompletedTask;

            //Act
            await mediatRMiddleware.Invoke(envelope, default, Next);

            //Assert
            Mock.Get(mockedMediator).Verify(x => x.Send(sentMessage, default), Times.Once);
        }

        [Fact]
        public void Should_throwExceptionForUnhandledMessageType()
        {
            //Arrange
            var mediatRMiddleware = new MediatRMiddleware(Mock.Of<IMediator>());
            var sentMessage = new {Field = "value"};

            var envelope =
                new MessagingEnvelope(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);


            Task Next() => Task.CompletedTask;

            //Act
            Task Action() => mediatRMiddleware.Invoke(envelope, default, Next);

            //Assert

            ((Func<Task>) Action).Should().Throw<ApplicationException>();
        }

        [Fact]
        public async void Should_callNextPipelineMiddleware()
        {
            //Arrange
            var mediatRMiddleware = new MediatRMiddleware(Mock.Of<IMediator>());
            var sentMessage = Mock.Of<IMockingEventMessage>();
            var isNextMiddlewareCalled = false;
            var envelope =
                new MessagingEnvelope<IMockingEventMessage>(new System.Collections.Generic.Dictionary<string, string>(),
                    sentMessage);

            Task Next()
            {
                isNextMiddlewareCalled = true;
                return Task.CompletedTask;
            }

            //Act
            await mediatRMiddleware.Invoke(envelope, default, Next);

            //Assert
            isNextMiddlewareCalled.Should().BeTrue();
        }

        public interface IMockingEventMessage : INotification
        {
        }

        public interface IMockingCommandMessage : IRequest
        {
        }

        public interface IMockingQueryMessage : IRequest<string>
        {
        }
    }
}