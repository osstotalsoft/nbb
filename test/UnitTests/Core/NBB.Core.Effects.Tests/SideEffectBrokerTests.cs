using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace NBB.Core.Effects.Tests
{
    public class SideEffectBrokerTests
    {
        [Fact]
        public async Task Should_handle_simple_side_effect()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddScoped<ISideEffectHandler<Simple.SideEffect, int>, Simple.Handler>();
            await using var container = services.BuildServiceProvider();
            var sut = new SideEffectBroker(container);

            //Act
            var result = await sut.Run<Simple.SideEffect, int>(new Simple.SideEffect(10));

            //Assert
            result.Should().Be(10);
        }

        [Fact]
        public async Task Should_handle_void_returning_side_effect()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddScoped<ISideEffectHandler<VoidReturning.SideEffect, Unit>, VoidReturning.Handler>();
            await using var container = services.BuildServiceProvider();
            var sut = new SideEffectBroker(container);

            //Act
            var sideEffectHandlerType = await sut.Run<VoidReturning.SideEffect, Unit>(new VoidReturning.SideEffect());

            //Assert
            sideEffectHandlerType.Should().Be(Unit.Value);
        }

        [Fact]
        public async Task Should_handle_generic_side_effect()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddScoped(typeof(Generic.Handler<>));
            await using var container = services.BuildServiceProvider();
            var sut = new SideEffectBroker(container);

            //Act
            var sideEffectHandler = await sut.Run<Generic.SideEffect<int>, int>(new Generic.SideEffect<int>());


            //Assert
            sideEffectHandler.Should().Be(0);
        }
    }

    public class Simple
    {
        public class SideEffect: ISideEffect<int>
        {
            public int Value { get; }

            public SideEffect(int value)
            {
                Value = value;
            }
        }

        public class Handler : ISideEffectHandler<SideEffect, int>
        {
            public Task<int> Handle(SideEffect sideEffect, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(sideEffect.Value);
            }
        }
    }

    public class Generic
    {
        public class SideEffect<TResponse> : ISideEffect<TResponse>, IAmHandledBy<Handler<TResponse>>
        {
        }

        public class Handler<TResponse> : ISideEffectHandler<SideEffect<TResponse>, TResponse>
        {
            public Task<TResponse> Handle(SideEffect<TResponse> sideEffect, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(default(TResponse));
            }
        }
    }

    public class VoidReturning
    {
        public class SideEffect: ISideEffect
        {
        }

        public class Handler : ISideEffectHandler<SideEffect, Unit>
        {
            public Task<Unit> Handle(SideEffect sideEffect, CancellationToken cancellationToken = default)
            {
                return Unit.Task;
            }
        }
    }
}
