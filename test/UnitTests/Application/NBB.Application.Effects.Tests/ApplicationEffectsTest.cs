using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NBB.Application.MediatR.Effects;
using Xunit;

namespace NBB.Application.Effects.Tests
{
    public class ApplicationEffectsTest
    {
        [Fact]
        public void AddMediatorEffects_should_register_MediatorSendQuery_SideEffectHandler()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IMediator>());

            //Act
            services.AddMediatorEffects();

            //Assert
            using var container = services.BuildServiceProvider();
            var handler = container.GetService(typeof(MediatorSendQuery.Handler<TestQuery>));
            handler.Should().NotBeNull();
        }

        [Fact]
        public async Task MediatorSendQuery_effect_handler_should_send_query_to_mediator()
        {
            //Arrange
            var mediator = new Mock<IMediator>();
            var sut = new MediatorSendQuery.Handler<TestResponse>(mediator.Object);
            var query = new TestQuery();
            var sideEffect = new MediatorSendQuery.SideEffect<TestResponse>(query);

            //Act
            var result = await sut.Handle(sideEffect);

            //Assert
            mediator.Verify(x=> x.Send(query, It.IsAny<CancellationToken>()), Times.Once);
        }

    }

    public class TestResponse
    {
    }

    public record TestQuery : IRequest<TestResponse>;
}
