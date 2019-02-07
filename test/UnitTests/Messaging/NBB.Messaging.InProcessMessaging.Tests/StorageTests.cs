using FluentAssertions;
using NBB.Messaging.DataContracts;
using NBB.Messaging.InProcessMessaging.Internal;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Messaging.InProcessMessaging.Tests
{
    public class StorageTests
    {
        public class TestMessage : IMessage
        {
            public IDictionary<string, string> Headers => new Dictionary<string, string>();

            public object Body => "test";
        }

        [Fact]
        public async Task Pub_Sub_Test()
        {
            //Arrange
            var topic = "x";
            var sut = new Storage();
            var msg = "ala bala ";
            var tokenSource = new CancellationTokenSource();


            //Act

            sut.Enqueue(msg, topic);
            await sut.AddSubscription(topic, message =>
            {
                //Assert
                message.Should().Be(msg);

                tokenSource.Cancel();
                return Task.CompletedTask;
            }, tokenSource.Token);
        }
    }
}
