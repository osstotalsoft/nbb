// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NBB.Application.MediatR.Effects;
using NBB.Core.Effects;
using Xunit;

namespace NBB.Application.Effects.Tests
{
    public class ApplicationEffectsTest
    {
        [Fact]
        public void AddMediatREffects_should_register_MediatorSendQuery_SideEffectHandler()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IMediator>());

            //Act
            services.AddMediatREffects();

            //Assert
            using var container = services.BuildServiceProvider();
            var handler = container.GetService(typeof(MediatorEffects.Send.QueryHandler<TestQuery>));
            handler.Should().NotBeNull();
        }

        [Fact]
        public async Task MediatorSendQuery_effect_handler_should_send_query_to_mediator()
        {
            //Arrange
            var mediator = new Mock<IMediator>();
            var sut = new MediatorEffects.Send.QueryHandler<TestResponse>(mediator.Object);
            var query = new TestQuery();
            var sideEffect = new MediatorEffects.Send.QuerySideEffect<TestResponse>(query);

            //Act
            var result = await sut.Handle(sideEffect);

            //Assert
            mediator.Verify(x=> x.Send(query, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void MediatorEff_Send_should_build_a_query_side_effect()
        {
            var query = new TestQuery();

            var effect = MediatorEff.Send(query);

            effect.Should().NotBeNull();
        }

        [Fact]
        public async Task MediatorEff_Send_should_resolve_the_request_handler_from_the_interpreter_scope()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ApplicationEffectsTest>());
            services.AddScoped<ScopedDependency>();
            services.AddEffects();
            services.AddMediatREffects();
            using var container = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

            foreach (var _ in new[] { 1, 2 })
            {
                using var scope = container.CreateScope();
                var interpreter = scope.ServiceProvider.GetRequiredService<IInterpreter>();

                //Act
                var dependency = await interpreter.Interpret(MediatorEff.Send(new GetScopedDependency()));

                //Assert
                dependency.Should().BeSameAs(scope.ServiceProvider.GetRequiredService<ScopedDependency>());
            }
        }

#pragma warning disable CS0618 // obsolete aliases kept for NBB 10 compatibility
        [Fact]
        public void Obsolete_AddMediatorEffects_should_still_register_the_handlers()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IMediator>());

            //Act
            services.AddMediatorEffects();

            //Assert
            using var container = services.BuildServiceProvider();
            container.GetService(typeof(MediatorEffects.Send.QueryHandler<TestQuery>)).Should().NotBeNull();
            NBB.Application.MediatR.Effects.Mediator.Send(new TestQuery()).Should().NotBeNull();
        }
#pragma warning restore CS0618

    }

    public class TestResponse
    {
    }

    public record TestQuery : IRequest<TestResponse>;

    public class ScopedDependency
    {
    }

    public record GetScopedDependency : IRequest<ScopedDependency>;

    public class GetScopedDependencyHandler(ScopedDependency dependency) : IRequestHandler<GetScopedDependency, ScopedDependency>
    {
        public Task<ScopedDependency> Handle(GetScopedDependency request, CancellationToken cancellationToken)
            => Task.FromResult(dependency);
    }
}
