using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace NBB.Core.Effects.Tests
{
    public class SideEffectHandlerFactoryTests
    {
        [Fact]
        public void Should_return_handler_for_simple_side_effect()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddScoped<ISideEffectHandler<Simple.SideEffect, int>, Simple.Handler>();
            using var container = services.BuildServiceProvider();
            var sut = new SideEffectHandlerFactory(container);

            //Act
            var sideEffectHandlerType = sut.GetSideEffectHandlerFor(new Simple.SideEffect());

            //Assert
            sideEffectHandlerType.Should().NotBeNull();
        }

        [Fact]
        public void Should_return_handler_for_void_returning_side_effect()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddScoped<ISideEffectHandler<VoidReturning.SideEffect>, VoidReturning.Handler>();
            using var container = services.BuildServiceProvider();
            var sut = new SideEffectHandlerFactory(container);

            //Act
            var sideEffectHandlerType = sut.GetSideEffectHandlerFor(new VoidReturning.SideEffect());

            //Assert
            sideEffectHandlerType.Should().NotBeNull();
        }

        [Fact]
        public void Should_return_handler_for_generic_side_effect()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddScoped(typeof(Generic.Handler<>));
            using var container = services.BuildServiceProvider();
            var sut = new SideEffectHandlerFactory(container);

            //Act
            var sideEffectHandler = sut.GetSideEffectHandlerFor(new Generic.SideEffect<int>());


            //Assert
            sideEffectHandler.Should().NotBeNull();
        }
    }

    public class Simple
    {
        public class SideEffect: ISideEffect<int>
        {

        }

        public class Handler : ISideEffectHandler<SideEffect, int>
        {
            public Task<int> Handle(SideEffect sideEffect, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
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
                throw new NotImplementedException();
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
                throw new NotImplementedException();
            }
        }
    }
}
