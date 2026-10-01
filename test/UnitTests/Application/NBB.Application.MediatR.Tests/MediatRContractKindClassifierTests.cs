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
        public void AddMediatRIntegration_should_register_the_publisher_and_the_classifier()
        {
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IPublisher>());

            services.AddMediatRIntegration();

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            scope.ServiceProvider.GetRequiredService<IEventPublisher>().Should().BeOfType<MediatREventPublisher>();
            sp.GetRequiredService<IContractKindClassifier>().Should().BeOfType<MediatRContractKindClassifier>();
        }

        [Fact]
        public void An_event_publisher_registered_after_AddMediatRIntegration_should_replace_it()
        {
            var services = new ServiceCollection();
            var eventPublisher = Mock.Of<IEventPublisher>();

            services.AddMediatRIntegration();
            services.AddSingleton(eventPublisher);

            using var sp = services.BuildServiceProvider();
            sp.GetRequiredService<IEventPublisher>().Should().BeSameAs(eventPublisher);
        }

        public record TestCommand : IRequest;
        public record TestQuery : IRequest<string>;
        public record TestEvent : INotification;
        public record PlainContract;
    }
}
