// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NBB.Core.Abstractions;
using Xunit;

namespace NBB.Application.MediatR.Tests
{
    public class MediatRContractKindClassifierTests
    {
        private readonly MediatRContractKindClassifier _sut = new();

        [Theory]
        [InlineData(typeof(TestCommand), ContractKind.Command)]
        [InlineData(typeof(TestQuery), ContractKind.Query)]
        [InlineData(typeof(TestEvent), ContractKind.Event)]
        [InlineData(typeof(PlainContract), ContractKind.Other)]
        public void Should_classify_mediatr_contracts(Type contractType, ContractKind expected)
            => _sut.Classify(contractType).Should().Be(expected);

        [Fact]
        public void AddMediatRIntegration_should_register_the_publisher_and_the_classifier_idempotently()
        {
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IPublisher>());

            services.AddMediatRIntegration();
            services.AddMediatRIntegration();

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            scope.ServiceProvider.GetServices<IEventPublisher>().Should().ContainSingle().Which.Should().BeOfType<MediatREventPublisher>();
            sp.GetServices<IContractKindClassifier>().Should().ContainSingle().Which.Should().BeOfType<MediatRContractKindClassifier>();
        }

        [Fact]
        public void AddMediatRIntegration_should_reject_another_event_publisher()
        {
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IEventPublisher>());

            var act = () => services.AddMediatRIntegration();

            act.Should().Throw<InvalidOperationException>().WithMessage("*single mediator library*");
        }

        public record TestCommand : IRequest;
        public record TestQuery : IRequest<string>;
        public record TestEvent : INotification;
        public record PlainContract;
    }
}
