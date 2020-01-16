﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Identification.Extensions;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Resolvers;
using Xunit;

namespace NBB.MultiTenancy.Identification.Tests.Extensions
{
    public class DependencyInjectionExtensionsTests
    {
        class FirstIdentifier : ITenantIdentifier
        {
            public Task<Guid> GetTenantIdAsync(string tenantToken)
            {
                throw new NotImplementedException();
            }
        }
        class FirstResolver : ITenantTokenResolver
        {
            public Task<string> GetTenantToken()
            {
                throw new NotImplementedException();
            }
        }

        private readonly IServiceCollection _serviceCollection;

        public DependencyInjectionExtensionsTests()
        {
            _serviceCollection = new ServiceCollection();
        }

        [Fact]
        public void Adding_Null_Resolver_Types_Should_Throw_ArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => _serviceCollection.AddTenantIdentificationStrategy<FirstIdentifier>(resolverTypes: null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Adding_Empty_Resolver_Types_Should_Throw_ArgumentException()
        {
            // Arrange

            // Act
            Action act = () => _serviceCollection.AddTenantIdentificationStrategy<FirstIdentifier>();

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Adding_Bad_Resolver_Types_Should_Throw_ArgumentException()
        {
            // Arrange

            // Act
            Action act = () => _serviceCollection.AddTenantIdentificationStrategy<FirstIdentifier>(typeof(FirstIdentifier));

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Adding_Resolver_Should_Be_In_Strategy()
        {
            // Arrange
            _serviceCollection.AddTenantIdentificationStrategy<FirstIdentifier>(typeof(FirstResolver));
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetService<TenantIdentificationStrategy>();

            // Assert
            result.Should().NotBeNull();
            result.TenantIdentifier.Should().BeOfType<FirstIdentifier>();
            result.TenantTokenResolvers.Should().HaveCount(1);
            result.TenantTokenResolvers.Should().AllBeOfType<FirstResolver>();
        }

        [Fact]
        public void Adding_Null_Identifier_With_Types_Should_Throw_ArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => _serviceCollection.AddTenantIdentificationStrategy((ITenantIdentifier)null, typeof(FirstResolver));

            // Arrange
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Adding_Identifier_With_Type_Should_Be_In_Strategy()
        {
            // Arrange
            _serviceCollection.AddTenantIdentificationStrategy(new FirstIdentifier(), typeof(FirstResolver));
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetService<TenantIdentificationStrategy>();

            // Assert
            result.Should().NotBeNull();
            result.TenantIdentifier.Should().BeOfType<FirstIdentifier>();
            result.TenantTokenResolvers.Should().HaveCount(1);
            result.TenantTokenResolvers.Should().AllBeOfType<FirstResolver>();
        }

        [Fact]
        public void Adding_Null_ImplementationFactory_With_Type_Should_Throw_ArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => _serviceCollection.AddTenantIdentificationStrategy((Func<IServiceProvider, ITenantIdentifier>)null, typeof(FirstResolver));

            // Arrange
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Adding_Implementation_Factory_With_Type_Should_Be_In_Strategy()
        {
            // Arrange
            _serviceCollection.AddTenantIdentificationStrategy(_ => new FirstIdentifier(), typeof(FirstResolver));
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetService<TenantIdentificationStrategy>();

            // Assert
            result.Should().NotBeNull();
            result.TenantIdentifier.Should().BeOfType<FirstIdentifier>();
            result.TenantTokenResolvers.Should().HaveCount(1);
            result.TenantTokenResolvers.Should().AllBeOfType<FirstResolver>();
        }

        [Fact]
        public void Adding_Null_Builder_Should_Throw_ArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => _serviceCollection.AddTenantIdentificationStrategy<FirstIdentifier>(builder: null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Adding_Resolver_Through_Builder_Should_Be_In_Strategy()
        {
            // Arrange
            _serviceCollection.AddTenantIdentificationStrategy<FirstIdentifier>(config => config.AddTenantTokenResolver<FirstResolver>());
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetService<TenantIdentificationStrategy>();

            // Assert
            result.Should().NotBeNull();
            result.TenantIdentifier.Should().BeOfType<FirstIdentifier>();
            result.TenantTokenResolvers.Should().HaveCount(1);
            result.TenantTokenResolvers.Should().AllBeOfType<FirstResolver>();
        }

        [Fact]
        public void Adding_Null_Identifier_With_Builder_Should_Throw_ArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => _serviceCollection.AddTenantIdentificationStrategy((ITenantIdentifier)null, config => config.AddTenantTokenResolver<FirstResolver>());

            // Arrange
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Adding_Identifier_With_Builder_Should_Be_In_Strategy()
        {
            // Arrange
            _serviceCollection.AddTenantIdentificationStrategy(new FirstIdentifier(), config => config.AddTenantTokenResolver<FirstResolver>());
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetService<TenantIdentificationStrategy>();

            // Assert
            result.Should().NotBeNull();
            result.TenantIdentifier.Should().BeOfType<FirstIdentifier>();
            result.TenantTokenResolvers.Should().HaveCount(1);
            result.TenantTokenResolvers.Should().AllBeOfType<FirstResolver>();
        }

        [Fact]
        public void Adding_Null_ImplementationFactory_With_Builder_Should_Throw_ArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => _serviceCollection.AddTenantIdentificationStrategy((Func<IServiceProvider, ITenantIdentifier>)null, config => config.AddTenantTokenResolver<FirstResolver>());

            // Arrange
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Adding_Implementation_Factory_With_Builder_Should_Be_In_Strategy()
        {
            // Arrange
            _serviceCollection.AddTenantIdentificationStrategy(_ => new FirstIdentifier(), config => config.AddTenantTokenResolver<FirstResolver>());
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetService<TenantIdentificationStrategy>();

            // Assert
            result.Should().NotBeNull();
            result.TenantIdentifier.Should().BeOfType<FirstIdentifier>();
            result.TenantTokenResolvers.Should().HaveCount(1);
            result.TenantTokenResolvers.Should().AllBeOfType<FirstResolver>();
        }
    }
}
