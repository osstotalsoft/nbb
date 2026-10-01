// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace NBB.Core.DependencyInjection.Tests
{
    public class SingleImplementationExtensionsTests
    {
        [Theory]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient)]
        public void Should_register_the_implementation_with_the_given_lifetime(ServiceLifetime lifetime)
        {
            var services = new ServiceCollection();

            services.AddSingleImplementation<IPort, Adapter>(lifetime);

            var descriptor = services.Single(d => d.ServiceType == typeof(IPort));
            descriptor.ImplementationType.Should().Be(typeof(Adapter));
            descriptor.Lifetime.Should().Be(lifetime);
        }

        [Fact]
        public void Should_ignore_registering_the_same_implementation_again()
        {
            var services = new ServiceCollection();

            services.AddSingleImplementation<IPort, Adapter>(ServiceLifetime.Singleton);
            services.AddSingleImplementation<IPort, Adapter>(ServiceLifetime.Singleton);

            services.Count(d => d.ServiceType == typeof(IPort)).Should().Be(1);
        }

        [Fact]
        public void Should_reject_a_different_implementation()
        {
            var services = new ServiceCollection();
            services.AddSingleImplementation<IPort, Adapter>(ServiceLifetime.Singleton);

            var act = () => services.AddSingleImplementation<IPort, OtherAdapter>(ServiceLifetime.Singleton);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage($"Cannot register {typeof(OtherAdapter).FullName} as {typeof(IPort).FullName}: {typeof(Adapter).FullName} is already registered.");
        }

        [Fact]
        public void Should_append_the_conflict_hint()
        {
            var services = new ServiceCollection();
            services.AddSingleImplementation<IPort, Adapter>(ServiceLifetime.Singleton);

            var act = () => services.AddSingleImplementation<IPort, OtherAdapter>(ServiceLifetime.Singleton, "Use a single adapter.");

            act.Should().Throw<InvalidOperationException>().WithMessage("*is already registered. Use a single adapter.");
        }

        [Fact]
        public void Should_compare_with_an_instance_registration()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IPort>(new Adapter());

            services.AddSingleImplementation<IPort, Adapter>(ServiceLifetime.Singleton);
            var act = () => services.AddSingleImplementation<IPort, OtherAdapter>(ServiceLifetime.Singleton);

            services.Count(d => d.ServiceType == typeof(IPort)).Should().Be(1);
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Should_treat_a_factory_registration_as_a_different_implementation()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IPort>(_ => new Adapter());

            var act = () => services.AddSingleImplementation<IPort, Adapter>(ServiceLifetime.Singleton);

            act.Should().Throw<InvalidOperationException>().WithMessage("*another implementation is already registered.");
        }

        public interface IPort
        {
        }

        public class Adapter : IPort
        {
        }

        public class OtherAdapter : IPort
        {
        }
    }
}
