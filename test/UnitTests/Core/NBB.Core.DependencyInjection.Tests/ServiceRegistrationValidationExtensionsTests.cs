// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace NBB.Core.DependencyInjection.Tests
{
    public class ServiceRegistrationValidationExtensionsTests
    {
        [Fact]
        public async Task RequireRegistration_should_fail_host_start_when_the_service_is_missing()
        {
            using var host = BuildHost(services => services.RequireRegistration(typeof(IRequired), "My feature", "call AddRequired()"));

            var act = () => host.StartAsync();

            await act.Should().ThrowAsync<OptionsValidationException>()
                .WithMessage($"*My feature requires {typeof(IRequired).FullName} to be registered: call AddRequired()*");
        }

        [Fact]
        public async Task RequireRegistration_should_accept_a_registration_made_after_the_call()
        {
            using var host = BuildHost(services => services
                .RequireRegistration(typeof(IRequired), "My feature", "call AddRequired()")
                .AddSingleton<IRequired, Required>());

            await host.StartAsync();
            await host.StopAsync();
        }

        [Theory]
        [InlineData(ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient)]
        public async Task RequireNonSingletonLifetime_should_accept_scoped_and_transient_registrations(ServiceLifetime lifetime)
        {
            using var host = BuildHost(services => services
                .RequireNonSingletonLifetime(typeof(IRequired), "My feature", "register it as scoped")
                .Add(new ServiceDescriptor(typeof(IRequired), typeof(Required), lifetime)));

            await host.StartAsync();
            await host.StopAsync();
        }

        [Fact]
        public async Task RequireNonSingletonLifetime_should_fail_host_start_for_a_singleton_registration()
        {
            using var host = BuildHost(services => services
                .AddSingleton<IRequired, Required>()
                .RequireNonSingletonLifetime(typeof(IRequired), "My feature", "register it as scoped"));

            var act = () => host.StartAsync();

            await act.Should().ThrowAsync<OptionsValidationException>()
                .WithMessage($"*My feature requires {typeof(IRequired).FullName} with the Scoped or Transient lifetime: register it as scoped*");
        }

        [Fact]
        public async Task RequireNonSingletonLifetime_should_not_report_a_missing_registration()
        {
            using var host = BuildHost(services => services.RequireNonSingletonLifetime(typeof(IRequired), "My feature", "register it as scoped"));

            await host.StartAsync();
            await host.StopAsync();
        }

        [Fact]
        public async Task Should_check_the_registration_resolved_by_the_container()
        {
            using var host = BuildHost(services => services
                .AddScoped<IRequired, Required>()
                .AddSingleton<IRequired, Required>()
                .RequireNonSingletonLifetime(typeof(IRequired), "My feature", "register it as scoped"));

            var act = () => host.StartAsync();

            await act.Should().ThrowAsync<OptionsValidationException>();
        }

        private static IHost BuildHost(Action<IServiceCollection> configure)
        {
            var builder = Host.CreateApplicationBuilder();
            configure(builder.Services);
            return builder.Build();
        }

        public interface IRequired
        {
        }

        public class Required : IRequired
        {
        }
    }
}
