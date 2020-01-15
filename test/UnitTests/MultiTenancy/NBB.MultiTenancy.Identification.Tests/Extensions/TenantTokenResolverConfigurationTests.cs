using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Identification.Extensions;
using NBB.MultiTenancy.Identification.Resolvers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NBB.MultiTenancy.Identification.Tests.Extensions
{
    public class TenantTokenResolverConfigurationTests
    {
        public class BadResolver
        { }

        public class MockResolver : ITenantTokenResolver
        {
            public Task<string> GetTenantToken()
            {
                throw new NotImplementedException();
            }
        }

        private readonly IServiceCollection _serviceCollection;

        public TenantTokenResolverConfigurationTests()
        {
            _serviceCollection = new ServiceCollection();
        }

        [Fact]
        public void Adding_Resolvers_By_Generic_Type_Registers_All_Of_Them()
        {
            // Arrange
            var sut = new TenantTokenResolverConfiguration(_serviceCollection);

            // Act
            sut.AddTenantTokenResolver<MockResolver>();
            sut.AddTenantTokenResolver<MockResolver>();
            sut.AddTenantTokenResolver<MockResolver>();

            // Arrange
            var result = sut.GetResolvers();
            result.Should().HaveCount(3);
        }

        [Fact]
        public void Adding_Null_Implementation_Throws_ArgumentNullException()
        {
            // Arrange
            var sut = new TenantTokenResolverConfiguration(_serviceCollection);

            // Act
            Action act = () => sut.AddTenantTokenResolver((ITenantTokenResolver)null);

            // Arrange
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Should_Add_Implementation_Type()
        {
            // Arrange
            var sut = new TenantTokenResolverConfiguration(_serviceCollection);
            var resolver = new MockResolver();

            // Act
            sut.AddTenantTokenResolver(resolver);
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            // Arrange;
            var result = sut.GetResolvers();
            result.Should().HaveCount(1);
            result.Should().AllBeOfType<MockResolver>();
            serviceProvider.GetServices<ITenantTokenResolver>().Should().HaveCount(1);
        }

        [Fact]
        public void Adding_Null_Type_Throws_ArgumentNullException()
        {
            // Arrange
            var sut = new TenantTokenResolverConfiguration(_serviceCollection);

            // Act
            Action act = () => sut.AddTenantTokenResolver((Type)null);

            // Arrange
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Adding_Bad_Type_Should_Throw_ArgumentException()
        {
            // Arrange
            var sut = new TenantTokenResolverConfiguration(_serviceCollection);
            var badType = typeof(BadResolver);

            // Act
            Action act = () => sut.AddTenantTokenResolver(badType);

            // Arrange
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Adding_Good_Type_Should_Add_Implementation_Type()
        {
            // Arrange
            var sut = new TenantTokenResolverConfiguration(_serviceCollection);

            // Act
            sut.AddTenantTokenResolver(typeof(MockResolver));
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            // Arrange;
            var result = sut.GetResolvers();
            result.Should().HaveCount(1);
            result.Should().AllBeOfType<MockResolver>();
            serviceProvider.GetServices<ITenantTokenResolver>().Should().HaveCount(1);
        }

        [Fact]
        public void Adding_Null_Implementation_Factory_Throws_ArgumentNullException()
        {
            // Arrange
            var sut = new TenantTokenResolverConfiguration(_serviceCollection);

            // Act
            Action act = () => sut.AddTenantTokenResolver((Func<IServiceProvider, MockResolver>)null);

            // Arrange
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Factory_Should_Add_Implementation_Type()
        {
            // Arrange
            var sut = new TenantTokenResolverConfiguration(_serviceCollection);
            var resolver = new MockResolver();

            // Act
            sut.AddTenantTokenResolver(_ => resolver);
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            // Arrange;
            var result = sut.GetResolvers();
            result.Should().HaveCount(1);
            result.Should().AllBeOfType<MockResolver>();
            serviceProvider.GetServices<ITenantTokenResolver>().Should().HaveCount(1);
        }
    }
}
