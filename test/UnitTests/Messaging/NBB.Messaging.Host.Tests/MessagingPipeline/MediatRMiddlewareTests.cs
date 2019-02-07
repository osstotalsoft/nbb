using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NBB.Core.Abstractions;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
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
            var mediatRMiddleware = new MediatRMiddleware(mockedMediator, Mock.Of<IMessageBusPublisher>());
            var sentMessage = Mock.Of<IMockingEventMessage>();
            var envelope = new MessagingEnvelope<IMockingEventMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() => Task.CompletedTask;

            //Act
            await mediatRMiddleware.Invoke(envelope, default(CancellationToken), Next);

            //Assert
            Mock.Get(mockedMediator).Verify(x => x.Publish<IEvent>(sentMessage, default(CancellationToken)), Times.Once);
        }

        [Fact]
        public async void Should_sendCommandsToMediatR()
        {
            //Arrange
            var mockedMediator = Mock.Of<IMediator>();
            var mediatRMiddleware = new MediatRMiddleware(mockedMediator, Mock.Of<IMessageBusPublisher>());
            var sentMessage = Mock.Of<IMockingCommandMessage>();
            var envelope = new MessagingEnvelope<IMockingCommandMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() => Task.CompletedTask;

            //Act
            await mediatRMiddleware.Invoke(envelope, default(CancellationToken), Next);

            //Assert
            Mock.Get(mockedMediator).Verify(x => x.Send(sentMessage, default(CancellationToken)), Times.Once);
        }

        [Fact]
        public async void Should_sendQueriesToMediatR()
        {
            //Arrange
            var mockedMediator = Mock.Of<IMediator>();
            var mediatRMiddleware = new MediatRMiddleware(mockedMediator, Mock.Of<IMessageBusPublisher>());
            var sentMessage = Mock.Of<IMockingQueryMessage>(x => x.GetResponseType() == typeof(string));
            var envelope = new MessagingEnvelope<IMockingQueryMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() => Task.CompletedTask;

            //Act
            await mediatRMiddleware.Invoke(envelope, default(CancellationToken), Next);

            //Assert
            Mock.Get(mockedMediator).Verify(x => x.Send(sentMessage, default(CancellationToken)), Times.Once);
        }

        [Fact]
        public async void Should_publishMediatRQueryResponseToBus()
        {
            //Arrange
            var queryResult = "RESULT";

            var mockedMediator = Mock.Of<IMediator>();
            Mock.Get(mockedMediator).Setup(x => x.Send(It.IsAny<IRequest<string>>(), default(CancellationToken)))
                .Returns(() => Task.FromResult(queryResult));

            var mockedBusPublisher = Mock.Of<IMessageBusPublisher>();
            var mediatRMiddleware = new MediatRMiddleware(mockedMediator, mockedBusPublisher);
            var sentMessage = Mock.Of<IMockingQueryMessage>(x => x.GetResponseType() == typeof(string));
            var envelope = new MessagingEnvelope<IMockingQueryMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() => Task.CompletedTask;

            //Act
            await mediatRMiddleware.Invoke(envelope, default(CancellationToken), Next);

            //Assert
            Mock.Get(mockedBusPublisher).Verify(x =>
                x.PublishAsync<IMessagingResponse>(It.Is<MessagingResponse<string>>(m => m.Response == queryResult),
                default(CancellationToken), null, null));
        }

        [Fact]
        public void Should_throwExceptionForUnhandledMessageType()
        {
            //Arrange
            var mediatRMiddleware = new MediatRMiddleware(Mock.Of<IMediator>(), Mock.Of<IMessageBusPublisher>());
            var sentMessage = Mock.Of<IMessage>();
            var envelope = new MessagingEnvelope<IMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);


            Task Next() => Task.CompletedTask;

            //Act
            Task Action() => mediatRMiddleware.Invoke(envelope, default(CancellationToken), Next);

            //Assert
            
            ((Func<Task>)Action).Should().Throw<ApplicationException>();
        }

        [Fact]
        public async void Should_callNextPipelineMiddleware()
        {
            //Arrange
            var mediatRMiddleware = new MediatRMiddleware(Mock.Of<IMediator>(), Mock.Of<IMessageBusPublisher>());
            var sentMessage = Mock.Of<IMockingEventMessage>();
            var isNextMiddlewareCalled = false;
            var envelope = new MessagingEnvelope<IMockingEventMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task Next() { isNextMiddlewareCalled = true; return Task.CompletedTask; }

            //Act
            await mediatRMiddleware.Invoke(envelope, default(CancellationToken), Next);

            //Assert
            isNextMiddlewareCalled.Should().BeTrue();
        }

        public interface IMockingEventMessage : IMessage, IEvent { }
        public interface IMockingCommandMessage : IMessage, ICommand { }
        public interface IMockingQueryMessage : IMessage, IQuery<string> { }
    }
}
