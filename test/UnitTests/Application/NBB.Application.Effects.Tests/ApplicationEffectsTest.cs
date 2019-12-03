using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using NBB.Application.Effects;
using NBB.Core.Abstractions;

namespace NBB.Application.Effects.Tests
{
    public class ApplicationEffectsTest
    {
        [Fact]
        public void AddMediatorEffects_should_register_MediatorSendQuery_SideEffectHandler()
        {
            //Arrange
            var services = new ServiceCollection();
            //services.AddSingleton(Mock.Of<IMessageBusPublisher>());

            //Act
            services.AddMediatorEffects();

            //Assert
            using var container = services.BuildServiceProvider();
            var handler = container.GetService(typeof(MediatorSendQuery.Handler<TestQuery>));
            handler.Should().NotBeNull();
        }

    }

    public class TestResponse
    {
    }

    public class TestQuery : IQuery<TestResponse>
    {
        public Type GetResponseType()
        {
            return typeof(TestResponse);
        }
    }
}
