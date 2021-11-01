// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using FluentAssertions;
using NBB.Messaging.InProcessMessaging.Internal;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Messaging.InProcessMessaging.Tests
{
    public class StorageTests
    {
        public class TestMessage
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
            var msg = System.Text.Encoding.UTF8.GetBytes("ala bala ");
            using var tokenSource = new CancellationTokenSource();
            //Act
            sut.Enqueue(msg, topic);
            await sut.AddSubscription(topic, message =>
            {
                //Assert
                message.Should().AllBeEquivalentTo(msg);

                tokenSource.Cancel();
                return Task.CompletedTask;
            }, tokenSource.Token);
        }
    }
}
