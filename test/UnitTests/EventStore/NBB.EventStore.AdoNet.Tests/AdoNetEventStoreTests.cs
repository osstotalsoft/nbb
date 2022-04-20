// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Configuration;
using NBB.MultiTenancy.Abstractions.Context;
using System;
using System.Collections.Generic;
using Xunit;

namespace NBB.EventStore.AdoNet.Tests;

public class AdoNetEventStoreTests
{
    [Fact]
    public void ShouldBindOptionsFromConfiguration()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddEventStore(es =>
        {
            es.UseNewtownsoftJson();
            es.UseAdoNetEventRepository(opts => opts.FromConfiguration());
        });
        const string myConnectionString = "myConnectionString";
        var myConfiguration = new Dictionary<string, string>
            {
                {"EventStore:NBB:ConnectionString", myConnectionString},
            };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();


        //Act
        using var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        using var scope = sp.CreateScope();
        var opts = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<EventStoreAdoNetOptions>>();

        //Asset
        opts.Value.ConnectionString.Should().Be(myConnectionString);
    }

    [Fact]
    public void ShouldBindOptionsFromMonoTenantConfiguration()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddEventStore(es =>
        {
            es.UseNewtownsoftJson();
            es.UseAdoNetEventRepository(opts => opts.From<ITenantConfiguration>((c, o)
                => o.ConnectionString = c.GetConnectionString("EventStore")));
        });
        const string myConnectionString = "myConnectionString";
        var myConfiguration = new Dictionary<string, string>
            {
                {"MultiTenancy:TenancyType","MonoTenant" },
                {"ConnectionStrings:EventStore", myConnectionString},
            };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddMultitenancy(configuration);
        services.AddDefaultTenantConfiguration();
        services.AddLogging();

        //Act
        using var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        using var scope = sp.CreateScope();
        var opts = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<EventStoreAdoNetOptions>>();

        //Asset
        opts.Value.ConnectionString.Should().Be(myConnectionString);
    }

    [Fact]
    public void ShouldBindOptionsFromMultiTenantConfiguration()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddEventStore(es =>
        {
            es.UseNewtownsoftJson();
            es.UseAdoNetEventRepository(opts => opts.From<ITenantConfiguration>((c, o) =>
            {
                o.ConnectionString = c.GetConnectionString("EventStore");
            }));
        });
        const string myConnectionString1 = "myConnectionString1";
        const string myConnectionString2 = "myConnectionString2";
        const string myTenantId1 = "68a448a2-e7d8-4875-8127-f18668217eb6";
        const string myTenantId2 = "333348a2-e7d8-4875-8127-f18668217333";
        var myConfiguration = new Dictionary<string, string>
            {
                {"MultiTenancy:TenancyType","MultiTenant" },
                {"MultiTenancy:Tenants:BCR:TenantId", myTenantId1},
                {"MultiTenancy:Tenants:BCR:Code", "BCR"},
                {"MultiTenancy:Tenants:BCR:ConnectionStrings:EventStore", myConnectionString1},
                {"MultiTenancy:Tenants:MBFS:TenantId", myTenantId2},
                {"MultiTenancy:Tenants:MBFS:Code", "MBFS"},
                {"MultiTenancy:Tenants:MBFS:ConnectionStrings:EventStore", myConnectionString2},
            };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddMultitenancy(configuration);
        services.AddDefaultTenantConfiguration();
        services.AddLogging();


        //Act
        using var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        IServiceScope CreateTenantScope(string tenantId, string tenantCode)
        {
            var scope = sp.CreateScope();

            var tca = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>().TenantContext =
                new TenantContext(new Tenant { TenantId = Guid.Parse(tenantId), Code = tenantCode });
            return scope;
        }


        //Assert
        using var scope1 = CreateTenantScope(myTenantId1, "BCR");
        var opts1 = scope1.ServiceProvider.GetRequiredService<IOptionsSnapshot<EventStoreAdoNetOptions>>();
        opts1.Value.ConnectionString.Should().Be(myConnectionString1);

        using var scope2 = CreateTenantScope(myTenantId2, "MBFS");
        var opts2 = scope2.ServiceProvider.GetRequiredService<IOptionsSnapshot<EventStoreAdoNetOptions>>();
        opts2.Value.ConnectionString.Should().Be(myConnectionString2);

    }
}
