using Moq;
using NBB.Core.Pipeline;
using NBB.Messaging.DataContracts;
using NBB.Messaging.Host.MessagingPipeline;
using System;
using Xunit;

namespace NBB.Messaging.Host.Tests.Pipeline
{
    public class MessagingPipelineExtensionsTests
    {
        [Fact]
        public void Should_UseCorrelationMiddleware()
        {
            //Arrange
            var pipelineBuilderMock = Mock.Of<IPipelineBuilder<MessagingEnvelope>>();

            //Act
            pipelineBuilderMock.UseCorrelationMiddleware();
            
            //Assert
            Mock.Get(pipelineBuilderMock).Verify(x => x.Use(It.IsAny<Func<PipelineDelegate<MessagingEnvelope>, PipelineDelegate<MessagingEnvelope>>>()));
        }

        [Fact]
        public void Should_UseDefaultResiliencyMiddleware()
        {
            //Arrange
            var pipelineBuilderMock = Mock.Of<IPipelineBuilder<MessagingEnvelope>>();

            //Act
            pipelineBuilderMock.UseDefaultResiliencyMiddleware();

            //Assert
            Mock.Get(pipelineBuilderMock).Verify(x => x.Use(It.IsAny<Func<PipelineDelegate<MessagingEnvelope>, PipelineDelegate<MessagingEnvelope>>>()));
        }

        [Fact]
        public void Should_UseExceptionHandlingMiddleware()
        {
            //Arrange
            var pipelineBuilderMock = Mock.Of<IPipelineBuilder<MessagingEnvelope>>();

            //Act
            pipelineBuilderMock.UseExceptionHandlingMiddleware();

            //Assert
            Mock.Get(pipelineBuilderMock).Verify(x => x.Use(It.IsAny<Func<PipelineDelegate<MessagingEnvelope>, PipelineDelegate<MessagingEnvelope>>>()));
        }

        [Fact]
        public void Should_UseMediatRMiddleware()
        {
            //Arrange
            var pipelineBuilderMock = Mock.Of<IPipelineBuilder<MessagingEnvelope>>();

            //Act
            pipelineBuilderMock.UseMediatRMiddleware();

            //Assert
            Mock.Get(pipelineBuilderMock).Verify(x => x.Use(It.IsAny<Func<PipelineDelegate<MessagingEnvelope>, PipelineDelegate<MessagingEnvelope>>>()));
        }
    }
}
