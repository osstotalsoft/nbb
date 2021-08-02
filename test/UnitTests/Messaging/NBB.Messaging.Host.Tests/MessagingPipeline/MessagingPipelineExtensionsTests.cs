using Moq;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using System;
using Xunit;

namespace NBB.Messaging.Host.Tests.MessagingPipeline
{
    public class MessagingPipelineExtensionsTests
    {
        [Fact]
        public void Should_UseCorrelationMiddleware()
        {
            //Arrange
            var pipelineBuilderMock = Mock.Of<IPipelineBuilder<MessagingContext>>();

            //Act
            pipelineBuilderMock.UseCorrelationMiddleware();
            
            //Assert
            Mock.Get(pipelineBuilderMock).Verify(x => x.Use(It.IsAny<Func<PipelineDelegate<MessagingContext>, PipelineDelegate<MessagingContext>>>()));
        }

        [Fact]
        public void Should_UseDefaultResiliencyMiddleware()
        {
            //Arrange
            var pipelineBuilderMock = Mock.Of<IPipelineBuilder<MessagingContext>>();

            //Act
            pipelineBuilderMock.UseDefaultResiliencyMiddleware();

            //Assert
            Mock.Get(pipelineBuilderMock).Verify(x => x.Use(It.IsAny<Func<PipelineDelegate<MessagingContext>, PipelineDelegate<MessagingContext>>>()));
        }

        [Fact]
        public void Should_UseExceptionHandlingMiddleware()
        {
            //Arrange
            var pipelineBuilderMock = Mock.Of<IPipelineBuilder<MessagingContext>>();

            //Act
            pipelineBuilderMock.UseExceptionHandlingMiddleware();

            //Assert
            Mock.Get(pipelineBuilderMock).Verify(x => x.Use(It.IsAny<Func<PipelineDelegate<MessagingContext>, PipelineDelegate<MessagingContext>>>()));
        }

        [Fact]
        public void Should_UseMediatRMiddleware()
        {
            //Arrange
            var pipelineBuilderMock = Mock.Of<IPipelineBuilder<MessagingContext>>();

            //Act
            pipelineBuilderMock.UseMediatRMiddleware();

            //Assert
            Mock.Get(pipelineBuilderMock).Verify(x => x.Use(It.IsAny<Func<PipelineDelegate<MessagingContext>, PipelineDelegate<MessagingContext>>>()));
        }
    }
}
