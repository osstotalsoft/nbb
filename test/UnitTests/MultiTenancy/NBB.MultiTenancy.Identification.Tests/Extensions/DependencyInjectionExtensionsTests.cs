using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Identification.Extensions;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Resolvers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NBB.MultiTenancy.Identification.Tests.Extensions
{
    public class DependencyInjectionExtensionsTests
    {
        public class MockIdentifier : ITenantIdentifier
        {
            public Task<Guid> GetTenantIdAsync(string tenantToken)
            {
                throw new NotImplementedException();
            }
        }
        public class MockSecondIdentifier : ITenantIdentifier
        {
            public Task<Guid> GetTenantIdAsync(string tenantToken)
            {
                throw new NotImplementedException();
            }
        }

        public class MockTenantTokenResolver : ITenantTokenResolver
        {
            public Task<string> GetTenantToken()
            {
                throw new NotImplementedException();
            }
        }

        public class MockSecondTenantTokenResolver : ITenantTokenResolver
        {
            public Task<string> GetTenantToken()
            {
                throw new NotImplementedException();
            }
        }

        public class MockBadClass
        { }

        [Fact]
        public void Should_Throw_ArgumentNullException_If_No_Types_Are_Provided()
        {
            // Arrange
            var sut = new ServiceCollection();

            // Act
            Action act = () => sut.AddResolverForIdentifier<MockIdentifier>(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Should_Throw_ArgumentException_If_No_Types_Are_Empty()
        {
            // Arrange
            var sut = new ServiceCollection();
            var types = new Type[0];

            // Act
            Action act = () => sut.AddResolverForIdentifier<MockIdentifier>(types);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Should_Throw_ArgumentException_If_Types_Contain_Interfaces()
        {
            // Arrange
            var sut = new ServiceCollection();
            var types = new[] { typeof(ITenantTokenResolver) };

            // Act
            Action act = () => sut.AddResolverForIdentifier<MockIdentifier>(types);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Should_Throw_ArgumentException_If_Types_Contain_Bad_Class()
        {
            // Arrange
            var sut = new ServiceCollection();
            var types = new[] { typeof(MockBadClass) };

            // Act
            Action act = () => sut.AddResolverForIdentifier<MockIdentifier>(types);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Same_Identifier_Types_Should_Be_Added_Once()
        {
            // Arrange
            var sut = new ServiceCollection();
            var types = new[] { typeof(MockTenantTokenResolver), typeof(MockTenantTokenResolver) };

            // Act
            sut.AddResolverForIdentifier<MockIdentifier>(types);
            sut.AddResolverForIdentifier<MockIdentifier>(types);

            // Assert
            var serviceProvider = sut.BuildServiceProvider();
            serviceProvider.GetServices<MockIdentifier>().Should().HaveCount(1);
        }

        [Fact]
        public void Created_Token_Identifier_Pair_Should_Have_Registered_Implementation()
        {
            // Arrange
            var sut = new ServiceCollection();
            var type = typeof(MockTenantTokenResolver);
            sut.AddResolverForIdentifier<MockIdentifier>(type);
            var serviceProvider = sut.BuildServiceProvider();

            // Act
            var tokenIdentifierPair = serviceProvider.GetService<TenantIdentificationPair>();

            // Assert
            tokenIdentifierPair.TenantIdentifier.Should().BeOfType<MockIdentifier>();
            tokenIdentifierPair.TenantTokenResolvers.Should().HaveCount(1);
            tokenIdentifierPair.TenantTokenResolvers.Should().AllBeOfType<MockTenantTokenResolver>();
        }

        [Fact]
        public void Should_Not_Mix_Types()
        {
            // Arrange
            var sut = new ServiceCollection();
            var firstType = typeof(MockTenantTokenResolver);
            var secondType = typeof(MockSecondTenantTokenResolver);

            sut.AddResolverForIdentifier<MockIdentifier>(firstType);
            sut.AddResolverForIdentifier<MockSecondIdentifier>(secondType);
            var serviceProvider = sut.BuildServiceProvider();

            // Act
            var pairs = serviceProvider.GetServices<TenantIdentificationPair>().ToList();
            var firstPair = pairs.First();
            var secondPair = pairs.Last();

            // Assert
            pairs.Should().HaveCount(2);
            firstPair.TenantTokenResolvers.Should().HaveCount(1);
            secondPair.TenantTokenResolvers.Should().HaveCount(1);

            firstPair.TenantIdentifier.Should().BeOfType<MockIdentifier>();
            secondPair.TenantIdentifier.Should().BeOfType<MockSecondIdentifier>();

            firstPair.TenantTokenResolvers.Should().AllBeOfType<MockTenantTokenResolver>();
            secondPair.TenantTokenResolvers.Should().AllBeOfType<MockSecondTenantTokenResolver>();
        }
    }
}
