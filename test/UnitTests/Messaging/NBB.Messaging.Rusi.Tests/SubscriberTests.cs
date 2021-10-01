// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Grpc.Core;
using Microsoft.Extensions.Options;
using Moq;
using NBB.Messaging.Abstractions;
using Proto.V1;
using System;
using System.Threading.Tasks;
using Xunit;
using Google.Protobuf;
using System.Text;

namespace NBB.Messaging.Rusi.Tests
{
    public class SubscriberTests
    {
        [Fact]
        public async Task TestSubscriber()
        {
            // Arrange
            var msgCount = 3;
            var payload = "{\"TestProp\":\"test1\"}";
            var msg = new ReceivedMessage()
            {
                Metadata = { { "aaa", "bbb" } },
                Data = ByteString.CopyFromUtf8(payload)
            };

            var mockResponseStream = Mock.Of<IAsyncStreamReader<ReceivedMessage>>(x => x.Current == msg);
            Mock.Get(mockResponseStream)
                .Setup(m => m.MoveNext(default))
                .Returns(() => Task.FromResult(msgCount-- > 0));

            var rusiClient = Mock.Of<Proto.V1.Rusi.RusiClient>();
            Mock.Get(rusiClient)
                .Setup(m => m.Subscribe(It.IsAny<SubscribeRequest>(), null, null, default))
                .Returns(new AsyncServerStreamingCall<ReceivedMessage>(mockResponseStream, Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }));

            var topic = "topic";
            var subscriber = new RusiMessagingTransport(rusiClient,
                                new OptionsWrapper<RusiOptions>(new RusiOptions() { PubsubName = "pubsub1" }));

            var handler = Mock.Of<Func<TransportReceiveContext, Task>>();

            // Act
            using var subscription = await subscriber.SubscribeAsync(topic, handler);
            await Task.Delay(100); //TODO: use more reliable awaiting

            // Assert
            Mock.Get(rusiClient)
                .Verify(x => x.Subscribe(It.Is<SubscribeRequest>(req => req.Topic == topic), null, null, default));

            Mock.Get(handler)
                .Verify(
                    handler => handler(It.Is<TransportReceiveContext>(m =>
                        Encoding.UTF8.GetString(((TransportReceivedData.PayloadBytesAndHeaders)m.ReceivedData).PayloadBytes) == payload &&
                        ((TransportReceivedData.PayloadBytesAndHeaders)m.ReceivedData).headers["aaa"] == "bbb")),
                    Times.Exactly(3));
        }

        public class TestMessage
        {
            public string TestProp { set; get; }
        }
    }
}
