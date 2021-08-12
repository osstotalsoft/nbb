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
        public void multiple_subscribers()
        {
            //Arrange
            var sub = new RangeMockSubscriber(1, 10);

            //Act
            var result1 = new List<int>();
            var result2 = new List<int>();

            var obs1 = sub.Observe<int>()
                .Select(x => x.Payload)
                .Where(x => x % 2 == 0);

            using var disp1 = obs1.Subscribe();
            using var disp2 = obs1.Subscribe(t => { result1.Add(t); });
            using var disp3 = obs1
                .Select(x => x * 2)
                .Where(x => x > 10)
                .Subscribe(t => { result2.Add(t); });

            sub.Start();

            //Assert
            result1.Should().BeEquivalentTo(2, 4, 6, 8, 10);
            result2.Should().BeEquivalentTo(12, 16, 20);
        }

        [Fact]
        public void combine_two_streams()
        {
            //Arrange
            var sub = new RangeMockSubscriber(1, 10);
            var obs2 = sub.Observe<int>();

            //Act
            var result1 = new List<string>();

            using var disp = sub.Observe<int>()
                .Select(x => x.Payload)
                .Where(x => x % 2 == 0)
                .Select(x => obs2.Select(y=> $"{x}-{y.Payload}"))
                .Concat()
                .Subscribe(t => { result1.Add(t); });

            sub.Start();

            //Assert
            //result1.Should().BeEquivalentTo(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);

        }
    }
}