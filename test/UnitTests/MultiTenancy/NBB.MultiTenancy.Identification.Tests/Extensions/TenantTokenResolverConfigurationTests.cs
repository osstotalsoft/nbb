// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using NBB.MultiTenancy.Identification.Extensions;
using NBB.MultiTenancy.Identification.Resolvers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NBB.MultiTenancy.Identification.Tests.Extensions
{
    public class TenantTokenResolverConfigurationTests
    {
        public class MockResolver : ITenantTokenResolver
        {
            public object[] Args { get; }

            public MockResolver()
            { }

            public MockResolver(int a, int b, int c)
            {
                Args = new object[] { a, b, c };
            }

            public Task<string> GetTenantToken()
            {
                throw new NotImplementedException();
            }
        }

        public class BadResolver
        { }

        private readonly TenantTokenResolverConfiguration _sut;

        public TenantTokenResolverConfigurationTests()
        {
            _sut = new TenantTokenResolverConfiguration();
        }

        [Fact]
        public void Adding_Resolver_Without_Params_Should_Create_Instance()
        {
            // Arrange
            _sut.AddTenantTokenResolver<MockResolver>();

            // Act
            var result = _sut.GetTenantTokenResolvers(null).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().HaveCount(1);
            result.First().Should().BeOfType<MockResolver>();
            ((MockResolver)result.First()).Args.Should().BeNull();
        }

        [Fact]
        public void Adding_Resolver_With_Params_Should_Create_Instance()
        {
            // Arrange
            var parameter = 1;
            _sut.AddTenantTokenResolver<MockResolver>(parameter, parameter, parameter);

            // Act
            var result = _sut.GetTenantTokenResolvers(null).ToList();
            var args = ((MockResolver)result.First()).Args;

            // Assert
            args.Should().NotBeNull();
            args.Should().HaveCount(3);
            args.Should().AllBeEquivalentTo(parameter);
        }

        [Fact]
        public void Adding_Null_Resolver_Type_Should_Throw_ArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => _sut.AddTenantTokenResolver(resolverType: null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Adding_Bad_Resolver_Type_Should_Throw_ArgumentException()
        {
            // Arrange

            // Act
            Action act = () => _sut.AddTenantTokenResolver(typeof(BadResolver));

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Adding_Resolver_Type_Without_Params_Should_Create_Instance()
        {
            // Arrange
            _sut.AddTenantTokenResolver(typeof(MockResolver));

            // Act
            var result = _sut.GetTenantTokenResolvers(null).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().HaveCount(1);
            result.First().Should().BeOfType<MockResolver>();
            ((MockResolver)result.First()).Args.Should().BeNull();
        }

        [Fact]
        public void Adding_Null_Resolver_Type_WithParams_Should_Throw_ArgumentNullException()
        {
            // Arrange
            var parameter = 1;

            // Act
            Action act = () => _sut.AddTenantTokenResolver(resolverType: null, parameter, parameter, parameter);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Adding_Bad_Resolver_Type_WithParams_Should_Throw_ArgumentException()
        {
            // Arrange
            var parameter = 1;

            // Act
            Action act = () => _sut.AddTenantTokenResolver(typeof(BadResolver), parameter, parameter, parameter);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Adding_Resolver_Type_With_Params_Should_Create_Instance()
        {
            // Arrange
            var parameter = 1;
            _sut.AddTenantTokenResolver(typeof(MockResolver), parameter, parameter, parameter);

            // Act
            var result = _sut.GetTenantTokenResolvers(null).ToList();
            var args = ((MockResolver)result.First()).Args;

            // Assert
            args.Should().NotBeNull();
            args.Should().HaveCount(3);
            args.Should().AllBeEquivalentTo(parameter);
        }

        [Fact]
        public void Adding_Null_Resolver_Instance_Should_Throw_ArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => _sut.AddTenantTokenResolver(tenantTokenResolver: null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Adding_Resolver_Instance_Should_Create_Instance()
        {
            // Arrange
            var resolver = new MockResolver();

            // Act
            _sut.AddTenantTokenResolver(resolver);
            var result = _sut.GetTenantTokenResolvers(null).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().HaveCount(1);
            result.First().Should().BeOfType<MockResolver>();
            ((MockResolver)result.First()).Args.Should().BeNull();
            result.First().Should().BeEquivalentTo(resolver);
        }

        [Fact]
        public void Adding_Null_Implementation_Factory_Should_Throw_ArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => _sut.AddTenantTokenResolver(implementationFactory: null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Adding_Implementation_Factory_Should_Create_Instance()
        {
            // Arrange
            var resolver = new MockResolver();
            ITenantTokenResolver ImplementationFactory(IServiceProvider _) => resolver;

            // Act
            _sut.AddTenantTokenResolver(ImplementationFactory);
            var result = _sut.GetTenantTokenResolvers(null).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().HaveCount(1);
            result.First().Should().BeOfType<MockResolver>();
            ((MockResolver)result.First()).Args.Should().BeNull();
            result.First().Should().BeEquivalentTo(resolver);
        }
    }
}
