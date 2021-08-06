using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
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


    }
}
