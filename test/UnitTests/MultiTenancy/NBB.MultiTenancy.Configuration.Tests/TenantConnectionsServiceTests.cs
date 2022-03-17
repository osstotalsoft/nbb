// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NBB.MultiTenancy.Abstractions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace NBB.MultiTenancy.Abstractions.Tests
{
    public class TenantConnectionsServiceTests
    {

        private IConfigurationBuilder GetConfigurationBuilder()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"MultiTenancy:Defaults:ConnectionStrings:Service1", "ConnectionString1Default"},
                {"MultiTenancy:Defaults:ConnectionStrings:Service2", "ConnectionString2Default"},
                {"MultiTenancy:Tenants:0:TenantId", "68a448a2-e7d8-4875-8127-f18668217eb6"},
                {"MultiTenancy:Tenants:0:ConnectionStrings:Service1", "ConnectionStringA"},
                {"MultiTenancy:Tenants:0:ConnectionStrings:Service2", "ConnectionStringB"},
                {"MultiTenancy:Tenants:1:TenantId", "da84628a-2925-4b69-9116-a90dd5a72b1f"},
                {"MultiTenancy:Tenants:1:ConnectionStrings:Service1", "ConnectionStringC"},
                {"MultiTenancy:Tenants:1:ConnectionStrings:Service2", "ConnectionStringD"},
            };
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration);
            return builder;
        }

        [Fact]
        public async Task tenant_connections_should_be_loaded()
        {
            // Arrange
            var configuration = GetConfigurationBuilder().Build();

            var sut = new TenantConnectionsService();

            var connections = await sut.GetConnectionStrings(configuration, "Service1");

            // Assert
            connections.Count.Should().Be(2);
            connections[0].Should().Be("ConnectionStringA");
            connections[1].Should().Be("ConnectionStringC");
        }

        [Fact]
        public async Task tenant_connections_should_be_distinct()
        {

            // Arrange
            var builder = GetConfigurationBuilder();
            var mySecondConfiguration = new Dictionary<string, string>
            {
                {"MultiTenancy:Tenants:2:TenantId", "288259b4-2086-493a-a206-e3517655120f"},
                {"MultiTenancy:Tenants:2:ConnectionStrings:Service1", "ConnectionStringA"},
                {"MultiTenancy:Tenants:2:ConnectionStrings:Service2", "ConnectionStringB"},
                {"MultiTenancy:Tenants:3:TenantId", "bc626b53-e3f5-4e1b-9e41-2b7130d74747"},
                {"MultiTenancy:Tenants:3:ConnectionStrings:Service1", "ConnectionStringC"},
                {"MultiTenancy:Tenants:3:ConnectionStrings:Service2", "ConnectionStringD"},
            };
            var configuration = builder
                .AddInMemoryCollection(mySecondConfiguration)
                .Build();

            var sut = new TenantConnectionsService();

            var connectionsService1 = await sut.GetConnectionStrings(configuration, "Service1");
            var connectionsService2 = await sut.GetConnectionStrings(configuration, "Service2");

            // Assert
            connectionsService1.Count.Should().Be(2);
            connectionsService1[0].Should().Be("ConnectionStringA");
            connectionsService1[1].Should().Be("ConnectionStringC");

            connectionsService2.Count.Should().Be(2);
            connectionsService2[0].Should().Be("ConnectionStringB");
            connectionsService2[1].Should().Be("ConnectionStringD");

        }
    }
}
