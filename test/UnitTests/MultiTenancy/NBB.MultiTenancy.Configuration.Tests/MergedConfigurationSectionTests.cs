// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Abstractions.Configuration;
using System.IO;
using System.Text;
using Xunit;

namespace NBB.MultiTenancy.Configuration.Tests;

public class MergedConfigurationSectionTests
{
    record ConnectionInfo(string Server, string Database, string UserName, string Password, string OtherParams);
    //{
    //    public string Server { get; set; }
    //}

    [Fact]
    public void ShouldMergeConfigurations()
    {
        //Arrange
        var defaultConfiguration = @"
        {
            ""Defaults"": {
                ""ConnectionStrings"": {
                    ""MyDb"": {
                        ""Server"": ""defaultServer"",
                        ""Database"": ""defaultDatabase"",
                        ""UserName"": ""defaultUserName"",
                        ""Password"": ""defaultPassword""
                    }
                }
            }
        }";

        var tenantConfiguration = @"
        {
            ""Tenant1"": {
                ""ConnectionStrings"": {
                    ""MyDb"": {
                        ""Server"": ""tenantServer"",
                        ""Database"": ""tenantDatabase"",
                        ""UserName"": ""tenantUserName"",
                        ""OtherParams"": ""tenantOtherParams""
                    }
                }
            }
        }";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(defaultConfiguration)))
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(tenantConfiguration)))
            .Build();
        var defaultSection = configuration.GetSection("Defaults");
        var tenantSection = configuration.GetSection("Tenant1");


        //Act
        IConfigurationSection mergedSection = new MergedConfigurationSection(tenantSection, defaultSection);

        //Assert
        mergedSection.GetValue<string>("ConnectionStrings:MyDb:Server").Should().Be("tenantServer");
        mergedSection.GetValue<string>("ConnectionStrings:MyDb:Password").Should().Be("defaultPassword");

        var ci = new ConnectionInfo("", "", "", "", "");
        mergedSection.GetSection("ConnectionStrings:MyDb").Bind(ci);
        ci.Should().Be(new ConnectionInfo("tenantServer", "tenantDatabase", "tenantUserName", "defaultPassword", "tenantOtherParams"));
    }
}
