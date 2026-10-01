// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NBB.Application.MediatR;
using NBB.Core.Abstractions;
using Xunit;

namespace NBB.Application.Mediator.Tests
{
    public class MediatorContractKindClassifierTests
    {
        private readonly MediatorContractKindClassifier _sut = new();

        [Theory]
        [InlineData(typeof(DoRequest), ContractKind.Command)]
        [InlineData(typeof(DoCommand), ContractKind.Command)]
        [InlineData(typeof(GetValue), ContractKind.Query)]
        [InlineData(typeof(CreateValue), ContractKind.Query)]
        [InlineData(typeof(QueryValue), ContractKind.Query)]
        [InlineData(typeof(SomethingHappened), ContractKind.Event)]
        [InlineData(typeof(PlainEvent), ContractKind.Other)]
        public void Should_classify_mediator_contracts(Type contractType, ContractKind expected)
            => _sut.Classify(contractType).Should().Be(expected);

        [Theory]
        [InlineData(typeof(DoRequest), typeof(MediatRContracts.DoRequest))]
        [InlineData(typeof(GetValue), typeof(MediatRContracts.GetValue))]
        [InlineData(typeof(SomethingHappened), typeof(MediatRContracts.SomethingHappened))]
        [InlineData(typeof(PlainEvent), typeof(MediatRContracts.PlainEvent))]
        public void Should_classify_like_the_mediatr_classifier_for_equivalent_contracts(Type mediatorContract, Type mediatRContract)
            => _sut.Classify(mediatorContract).Should().Be(new MediatRContractKindClassifier().Classify(mediatRContract));

        [Fact]
        public void AddMediatorIntegration_should_register_the_classifier()
        {
            var services = new ServiceCollection();

            services.AddMediatorIntegration();

            using var sp = services.BuildServiceProvider();
            sp.GetRequiredService<IContractKindClassifier>().Should().BeOfType<MediatorContractKindClassifier>();
        }
    }

    public static class MediatRContracts
    {
        public record DoRequest : global::MediatR.IRequest;
        public record GetValue : global::MediatR.IRequest<string>;
        public record SomethingHappened : global::MediatR.INotification;
        public record PlainEvent;
    }
}
