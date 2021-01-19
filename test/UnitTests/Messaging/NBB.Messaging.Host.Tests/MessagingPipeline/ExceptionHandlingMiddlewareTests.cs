using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host.MessagingPipeline;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Messaging.Host.Tests.Pipeline
{
    public class ExceptionHandlingMiddlewareTests
    {
        [Fact]
        public async void Should_logSuccessMessage()
        {
            //Arrange
            var mockedLogger = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();
            var correlationMiddleWare = new ExceptionHandlingMiddleware(mockedLogger);
            var sentMessage = new { Field = "value"};
            var envelope = new MessagingEnvelope(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task next() => Task.CompletedTask;

            //Act
            await correlationMiddleWare.Invoke(envelope, default, next);

            //Assert
            VerifyLog(mockedLogger, LogLevel.Information, "processed in");
        }

        [Fact]
        public async void Should_logErrorMessageWhenExceptionIsThrown()
        {
            //Arrange
            var mockedLogger = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();
            var correlationMiddleWare = new ExceptionHandlingMiddleware(mockedLogger);
            var sentMessage = new { Field = "value"};
            var envelope = new MessagingEnvelope(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);

            Task next() => throw new ApplicationException();

            //Act
            try
            {
                await correlationMiddleWare.Invoke(envelope, default, next);
            }
            catch
            {
                // ignored
            }

            //Assert
            VerifyLog(mockedLogger, LogLevel.Error, nameof(ApplicationException));
        }

        [Fact]
        public async void Should_callNextPipelineMiddleware()
        {
            //Arrange
            var executionTimeMiddleware = new ExceptionHandlingMiddleware(Mock.Of<ILogger<ExceptionHandlingMiddleware>>());
            var sentMessage = new { Field = "value"};
            bool isNextMiddlewareCalled = false;
            var envelope = new MessagingEnvelope(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);


            Task next() { isNextMiddlewareCalled = true; return Task.CompletedTask; }

            //Act
            await executionTimeMiddleware.Invoke(envelope, default, next);

            //Assert
            isNextMiddlewareCalled.Should().BeTrue();
        }

        private void VerifyLog(ILogger<ExceptionHandlingMiddleware> mockedLogger, LogLevel logLevel, string containedString)
        {
            Mock.Get(mockedLogger).Verify(x => x.Log(logLevel, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains(containedString)),
                null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }
    }
}
