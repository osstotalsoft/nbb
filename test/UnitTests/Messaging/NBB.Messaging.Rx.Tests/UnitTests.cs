using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reactive.Linq;
using System.Reflection;
using FluentAssertions;
using Moq;
using Xunit;

namespace NBB.Messaging.Rx.Tests
{
    public class UnitTests
    {

        [Fact]
        public void Should_deserialize_messages_using_constructor_with_optional_params()
        {
            //Arrange
            var sub = new RangeMockSubscriber(1, 100);
            var obs1 = sub.Observe<int>();
            var obs2 = sub.Observe<int>();

            //Act
            var dis =
                sub.Observe<int>()
                    .Select(x => x.Payload)
                    .Select(x => obs2)
                    .Concat()
                    .Subscribe();

            sub.Start();

            //Assert
        }
    }
}