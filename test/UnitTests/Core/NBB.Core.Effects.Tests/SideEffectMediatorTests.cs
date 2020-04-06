using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace NBB.Core.Effects.Tests
{
    public class SideEffectMediatorTests
    {
        [Fact]
        public async Task Should_handle_simple_side_effect()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddScoped<ISideEffectHandler<Simple.SideEffect, int>, Simple.Handler>();
            using var container = services.BuildServiceProvider();
            var sut = new SideEffectMediator(container);

            //Act
            var result = await sut.Run(new Simple.SideEffect(10));

            //Assert
            result.Should().Be(10);
        }

        [Fact]
        public async Task Should_handle_void_returning_side_effect()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddScoped<ISideEffectHandler<VoidReturning.SideEffect>, VoidReturning.Handler>();
            using var container = services.BuildServiceProvider();
            var sut = new SideEffectMediator(container);

            //Act
            var sideEffectHandlerType = await sut.Run(new VoidReturning.SideEffect());

            //Assert
            sideEffectHandlerType.Should().Be(Unit.Value);
        }

        [Fact]
        public async Task Should_handle_generic_side_effect()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddScoped(typeof(Generic.Handler<>));
            using var container = services.BuildServiceProvider();
            var sut = new SideEffectMediator(container);

            //Act
            var sideEffectHandler = await sut.Run(new Generic.SideEffect<int>());


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

        public class Handler : ISideEffectHandler<SideEffect>
        {
            public Task Handle(SideEffect sideEffect, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}
