using System.Linq;
using FluentAssertions;
using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;

namespace NBB.Messaging.DataContracts.Tests
{
    public class TopicNameResolverTests
    {
        const string TestTopic = "TestTopic";

        [TopicName(TestTopic)]
        public class TestMessage
        {
        }

        [Fact]
        public void ShouldResolveTopicName()
        {
            //Arrange
            var messageType = typeof(TestMessage);
            var topicNameResolver = messageType.GetCustomAttributes(typeof(TopicNameResolverAttribute), true).FirstOrDefault() as TopicNameResolverAttribute;
            var configuration = Mock.Of<IConfiguration>();

            //Act
            var resolvedTopicName = topicNameResolver?.ResolveTopicName(messageType, configuration);

            //Assert
            resolvedTopicName.Should().Be(TestTopic);
        }
    }
}
