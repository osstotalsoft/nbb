using System;
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
            Action act = () => sut.AddResolversForIdentifier<MockIdentifier>(resolvers: null);

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
            Action act = () => sut.AddResolversForIdentifier<MockIdentifier>(types);

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
            Action act = () => sut.AddResolversForIdentifier<MockIdentifier>(types);

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
            Action act = () => sut.AddResolversForIdentifier<MockIdentifier>(types);

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
            sut.AddResolversForIdentifier<MockIdentifier>(types);
            sut.AddResolversForIdentifier<MockIdentifier>(types);

            // Assert
            var serviceProvider = sut.BuildServiceProvider();
            serviceProvider.GetServices<MockIdentifier>().Should().HaveCount(1);
        }

        [Fact]
        public void Created_Token_Identifier_Strategy_Should_Have_Registered_Implementation()
        {
            // Arrange
            var sut = new ServiceCollection();
            var type = typeof(MockTenantTokenResolver);
            sut.AddResolversForIdentifier<MockIdentifier>(type);
            var serviceProvider = sut.BuildServiceProvider();

            // Act
            var identificationStrategy = serviceProvider.GetService<TenantIdentificationStrategy>();

            // Assert
            identificationStrategy.TenantIdentifier.Should().BeOfType<MockIdentifier>();
            identificationStrategy.TenantTokenResolvers.Should().HaveCount(1);
            identificationStrategy.TenantTokenResolvers.Should().AllBeOfType<MockTenantTokenResolver>();
        }

        [Fact]
        public void Created_Token_Identifier_Strategy_From_Instance_Should_Have_Registered_Implementation()
        {
            // Arrange
            var sut = new ServiceCollection();
            var type = typeof(MockTenantTokenResolver);
            var identifier = new MockIdentifier();
            sut.AddResolversForIdentifier(identifier, type);
            var serviceProvider = sut.BuildServiceProvider();

            // Act
            var identificationStrategy = serviceProvider.GetService<TenantIdentificationStrategy>();

            // Assert
            identificationStrategy.TenantIdentifier.Should().BeOfType<MockIdentifier>();
            identificationStrategy.TenantTokenResolvers.Should().HaveCount(1);
            identificationStrategy.TenantTokenResolvers.Should().AllBeOfType<MockTenantTokenResolver>();
        }

        [Fact]
        public void Created_Token_Identifier_Strategy_From_Factory_Should_Have_Registered_Implementation()
        {
            // Arrange
            var sut = new ServiceCollection();
            var type = typeof(MockTenantTokenResolver);
            sut.AddResolversForIdentifier(_ => new MockIdentifier(), type);
            var serviceProvider = sut.BuildServiceProvider();

            // Act
            var identificationStrategy = serviceProvider.GetService<TenantIdentificationStrategy>();

            // Assert
            identificationStrategy.TenantIdentifier.Should().BeOfType<MockIdentifier>();
            identificationStrategy.TenantTokenResolvers.Should().HaveCount(1);
            identificationStrategy.TenantTokenResolvers.Should().AllBeOfType<MockTenantTokenResolver>();
        }

        [Fact]
        public void Should_Not_Mix_Types()
        {
            // Arrange
            var sut = new ServiceCollection();
            var firstType = typeof(MockTenantTokenResolver);
            var secondType = typeof(MockSecondTenantTokenResolver);

            sut.AddResolversForIdentifier<MockIdentifier>(firstType);
            sut.AddResolversForIdentifier<MockSecondIdentifier>(secondType);
            var serviceProvider = sut.BuildServiceProvider();

            // Act
            var pairs = serviceProvider.GetServices<TenantIdentificationStrategy>().ToList();
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

        [Fact]
        public void Same_Identifier_Instances_Types_Should_Be_Added_Once()
        {
            // Arrange
            var sut = new ServiceCollection();
            var types = new[] { typeof(MockTenantTokenResolver), typeof(MockTenantTokenResolver) };
            var identifier = new MockIdentifier();
            var diffIdentifier = new MockIdentifier();

            // Act
            sut.AddResolversForIdentifier(identifier, types);
            sut.AddResolversForIdentifier(identifier, types);
            sut.AddResolversForIdentifier(diffIdentifier, types);

            // Assert
            var serviceProvider = sut.BuildServiceProvider();
            serviceProvider.GetServices<MockIdentifier>().Should().HaveCount(1);
        }

        [Fact]
        public void Same_Types_Returned_From_Factory_Should_Be_Added_Once()
        {
            // Arrange
            var sut = new ServiceCollection();
            var types = new[] { typeof(MockTenantTokenResolver), typeof(MockTenantTokenResolver) };

            // Act
            sut.AddResolversForIdentifier(_ => new MockIdentifier(), types);
            sut.AddResolversForIdentifier(_ => new MockIdentifier(), types);

            // Assert
            var serviceProvider = sut.BuildServiceProvider();
            serviceProvider.GetServices<MockIdentifier>().Should().HaveCount(1);
        }

        [Fact]
        public void Should_Register_Types_From_Config()
        {
            // Arrange
            var sut = new ServiceCollection();
            sut.AddResolversForIdentifier<MockIdentifier>(builder: config => config.AddTenantTokenResolver<MockTenantTokenResolver>());
            var serviceProvider = sut.BuildServiceProvider();

            // Act
            var identificationStrategy = serviceProvider.GetService<TenantIdentificationStrategy>();

            // Assert
            identificationStrategy.TenantIdentifier.Should().BeOfType<MockIdentifier>();
            identificationStrategy.TenantTokenResolvers.Should().HaveCount(1);
            identificationStrategy.TenantTokenResolvers.Should().AllBeOfType<MockTenantTokenResolver>();
        }

        [Fact]
        public void Should_Throw_ArgumentNullException_If_Builder_Null()
        {
            // Arrange
            var sut = new ServiceCollection();

            // Act
            Action act = () => sut.AddResolversForIdentifier<MockIdentifier>(builder: null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Should_Throw_ArgumentException_If_Builder_Does_Not_Register_Resolvers()
        {
            // Arrange
            var sut = new ServiceCollection();

            // Act
            Action act = () => sut.AddResolversForIdentifier<MockIdentifier>(_ => { });

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}
