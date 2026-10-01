// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NBB.Core.Abstractions;
using NBB.Messaging.Abstractions;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.Identification.Services;
using Xunit;

namespace NBB.Messaging.MultiTenancy.Tests
{
    public class TenantMiddlewareTests
    {
        [Fact]
        public async Task Should_skip_events_when_the_tenant_cannot_be_identified()
        {
            //Arrange
            var identification = new Mock<ITenantIdentificationService>();
            identification.Setup(x => x.TryGetTenantIdAsync()).ReturnsAsync((Guid?)null);
            var sut = CreateSut(identification.Object, new Mock<ITenantRepository>().Object, new FakeClassifier(ContractKind.Event));
            var nextCalled = false;

            //Act
            await sut.Invoke(Context(new TestMessage()), default, () => { nextCalled = true; return Task.CompletedTask; });

            //Assert
            nextCalled.Should().BeFalse();
            identification.Verify(x => x.GetTenantIdAsync(), Times.Never);
        }

        [Fact]
        public async Task Should_process_events_for_identified_tenants()
        {
            //Arrange
            var tenant = new Tenant(Guid.NewGuid(), "t1");
            var identification = Mock.Of<ITenantIdentificationService>(x => x.TryGetTenantIdAsync() == Task.FromResult<Guid?>(tenant.TenantId));
            var repository = Mock.Of<ITenantRepository>(x => x.TryGet(tenant.TenantId, It.IsAny<CancellationToken>()) == Task.FromResult(tenant));
            var tenantContextAccessor = new TenantContextAccessor();
            var sut = CreateSut(identification, repository, new FakeClassifier(ContractKind.Event), tenantContextAccessor);
            Tenant tenantInNext = null;

            //Act
            await sut.Invoke(Context(new TestMessage()), default, () => { tenantInNext = tenantContextAccessor.TenantContext.Tenant; return Task.CompletedTask; });

            //Assert
            tenantInNext.Should().Be(tenant);
        }

        [Theory]
        [InlineData(ContractKind.Command)]
        [InlineData(ContractKind.Query)]
        [InlineData(ContractKind.Other)]
        public async Task Should_require_a_tenant_for_messages_that_are_not_events(ContractKind kind)
        {
            //Arrange
            var identification = new Mock<ITenantIdentificationService>();
            identification.Setup(x => x.GetTenantIdAsync()).ThrowsAsync(new TenantNotFoundException());
            var sut = CreateSut(identification.Object, new Mock<ITenantRepository>().Object, new FakeClassifier(kind));

            //Act
            var act = () => sut.Invoke(Context(new TestMessage()), default, () => Task.CompletedTask);

            //Assert
            await act.Should().ThrowAsync<TenantNotFoundException>();
            identification.Verify(x => x.TryGetTenantIdAsync(), Times.Never);
        }

        [Fact]
        public async Task Should_not_require_a_classifier_for_publisher_only_apps()
        {
            //Arrange
            var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
            builder.Services.AddSingleton(Mock.Of<IMessageBusPublisher>());
            builder.Services.AddMultiTenantMessaging();
            using var host = builder.Build();

            //Act
            await host.StartAsync();

            //Assert
            await host.StopAsync();
        }

        [Fact]
        public void Should_fail_to_activate_the_middleware_without_a_classifier()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ITenantContextAccessor, TenantContextAccessor>();
            services.AddSingleton(Mock.Of<ITenantIdentificationService>());
            services.AddSingleton(Mock.Of<ITenantRepository>());
            services.AddOptions<TenancyHostingOptions>();
            services.AddLogging();
            using var sp = services.BuildServiceProvider();

            //Act
            var act = () => ActivatorUtilities.CreateInstance<TenantMiddleware>(sp);

            //Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*IContractKindClassifier*");
        }

        private static TenantMiddleware CreateSut(ITenantIdentificationService identification, ITenantRepository repository,
            IContractKindClassifier classifier, ITenantContextAccessor tenantContextAccessor = null)
            => new(tenantContextAccessor ?? new TenantContextAccessor(),
                identification,
                Options.Create(new TenancyHostingOptions { TenancyType = TenancyType.MultiTenant }),
                repository,
                classifier,
                NullLogger<TenantMiddleware>.Instance);

        private static MessagingContext Context(object payload)
            => new(new MessagingEnvelope(new Dictionary<string, string>(), payload), "topic", null);

        private record TestMessage;

        private class FakeClassifier(ContractKind kind) : IContractKindClassifier
        {
            public ContractKind Classify(Type contractType) => kind;
        }
    }
}
