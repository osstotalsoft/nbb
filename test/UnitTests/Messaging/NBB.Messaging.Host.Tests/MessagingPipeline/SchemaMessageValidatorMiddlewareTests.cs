// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using NBB.Messaging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Messaging.Host.Tests.MessagingPipeline
{
    public class SchemaMessageValidatorMiddlewareTests
    {
        [Fact]
        public async Task Should_throwExceptionWhenAHeaderFieldIsNotProvided()
        {
            //Arrange
            var schemaMessageValidationMiddleware = new SchemaMessageValidatorMiddleware();
            var sentMessage = new { Field = "value" };
            var headers = new System.Collections.Generic.Dictionary<string, string>()
            {
                [MessagingHeaders.MessageType] = "String",
                [MessagingHeaders.Source] = "Tasks",
                [MessagingHeaders.PublishTime] = DateTime.Now.ToString(),
                [MessagingHeaders.MessageId] = Guid.NewGuid().ToString(),
                [MessagingHeaders.StreamId] = Guid.NewGuid().ToString()
            };
            var envelope = new MessagingEnvelope(headers, sentMessage);

            Task next() => Task.CompletedTask;

            //Act + Assert
            await Assert.ThrowsAsync<Exception>(() => schemaMessageValidationMiddleware.Invoke(new MessagingContext(envelope, string.Empty, null), default, next));
        }

        [Fact]
        public async Task Should_callNextPipelineMiddlewareWhenAllHeaderFieldsAreProvided()
        {
            //Arrange
            bool isNextMiddlewareCalled = false;
            var schemaMessageValidationMiddleware = new SchemaMessageValidatorMiddleware();
            var sentMessage = new { Field = "value" };
            var headers = new System.Collections.Generic.Dictionary<string, string>()
            {
                [MessagingHeaders.MessageType] = "String",
                [MessagingHeaders.Source] = "Tasks",
                [MessagingHeaders.PublishTime] = DateTime.Now.ToString(),
                [MessagingHeaders.MessageId] = Guid.NewGuid().ToString(),
                [MessagingHeaders.StreamId] = Guid.NewGuid().ToString(),
                [MessagingHeaders.CorrelationId] = Guid.NewGuid().ToString()
            };
            var envelope = new MessagingEnvelope(headers, sentMessage);

            Task Next() { isNextMiddlewareCalled = true; return Task.CompletedTask; }

            //Act
            await schemaMessageValidationMiddleware.Invoke(new MessagingContext(envelope, string.Empty, null), default, Next);

            // Assert
            isNextMiddlewareCalled.Should().BeTrue();
        }
    }
}
