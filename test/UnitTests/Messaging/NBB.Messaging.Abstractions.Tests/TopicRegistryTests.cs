using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NBB.Core.Abstractions;
using NBB.Messaging.DataContracts;
using Xunit;

namespace NBB.Messaging.Abstractions.Tests
{
    public class DefaultTopicRegistryTests
    {
        public class MyResolverAttribute : TopicNameResolverAttribute
        {
            public override string ResolveTopicName(Type messageType, IConfiguration configuration)
            {
                return "cucu";
            }
        }

        [MyResolver]
        public class MyEvent
        {
        }


        [Fact]
        public void Should_resolve_topic_name_from_custom_resolver()
        {
            //Arrange
            var sut = new DefaultTopicRegistry(Mock.Of<IConfiguration>());

            //Act
            var topicName = sut.GetTopicForMessageType(typeof(MyEvent));

            //Assert
            topicName.Should().Be("cucu");
        }

        [Fact]
        public void Should_get_command_topic()
        {
            //Arrange
            var sut = new DefaultTopicRegistry(Mock.Of<IConfiguration>());

            //Act
            var topicName = sut.GetTopicForMessageType(typeof(CustomCommand));

            //Assert
            topicName.Should().Be($"ch.commands.{typeof(CustomCommand).GetLongPrettyName()}");
        }

        [Fact]
        public void Should_get_event_topic()
        {
            //Arrange
            var sut = new DefaultTopicRegistry(Mock.Of<IConfiguration>());

            //Act
            var topicName = sut.GetTopicForMessageType(typeof(CustomEvent));

            //Assert
            topicName.Should().Be($"ch.events.{typeof(CustomEvent).GetLongPrettyName()}");
        }

        [Fact]
        public void Should_get_query_topic()
        {
            //Arrange
            var sut = new DefaultTopicRegistry(Mock.Of<IConfiguration>());

            //Act
            var topicName = sut.GetTopicForMessageType(typeof(CustomQuery));

            //Assert
            topicName.Should().Be($"ch.queries.{typeof(CustomQuery).GetLongPrettyName()}");
        }

        [Fact]
        public void Should_get_custom_topic()
        {
            //Arrange
            var sut = new DefaultTopicRegistry(Mock.Of<IConfiguration>());

            //Act
            var topicName = sut.GetTopicForMessageType(typeof(string));

            //Assert
            topicName.Should().Be($"ch.messages.{typeof(string).GetLongPrettyName()}");
        }

        [Fact]
        public void Shold_return_null_for_null_topic()
        {
            //Arrange
            var sut = new DefaultTopicRegistry(Mock.Of<IConfiguration>());

            //Act
            var topicName = sut.GetTopicForName(null, false);

            //Assert
            topicName.Should().BeNull();
        }
    }

    class CustomCommand: ICommand
    {

    }

    class CustomEvent : IEvent
    {
        public Guid EventId => Guid.NewGuid();
    }

    class CustomQuery : IQuery
    {
        public Type GetResponseType()
        {
            return typeof(string);
        }
    }
}
