// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Xunit;

namespace NBB.Core.Configuration.Tests
{
    public class ConnectionStringConfigurationTests
    {
        private const string IdentityClientId = "3f2c1a9e-6b7d-4e8f-9a0b-1c2d3e4f5a6b";

        [Fact]
        public void Should_return_null_when_connection_string_is_missing()
        {
            // Arrange
            var configuration = BuildConfiguration(new() { { "ConnectionStrings:Other", "Server=s" } });

            // Act
            var connectionString = configuration.ResolveConnectionString("App_Database");

            // Assert
            connectionString.Should().BeNull();
        }

        [Fact]
        public void Should_return_string_value()
        {
            // Arrange
            var configuration = BuildConfiguration(new()
            {
                { "ConnectionStrings:App_Database", "Server=server0;Database=db0;User Id=web;Password=pass;MultipleActiveResultSets=true" }
            });

            // Act
            var connectionString = configuration.ResolveConnectionString("App_Database");

            // Assert
            connectionString.Should().Be("Server=server0;Database=db0;User Id=web;Password=pass;MultipleActiveResultSets=true");
        }

        [Fact]
        public void Should_build_from_segments()
        {
            // Arrange
            var configuration = BuildConfiguration(LegacySegments());

            // Act
            var connectionString = configuration.ResolveConnectionString("App_Database");

            // Assert
            connectionString.Should().Be("Server=server1;Database=db1;User Id=web;Password=pass;MultipleActiveResultSets=true");
        }

        [Fact]
        public void Should_build_from_segments_without_other_params()
        {
            // Arrange
            var segments = LegacySegments();
            segments.Remove("ConnectionStrings:App_Database:OtherParams");
            var configuration = BuildConfiguration(segments);

            // Act
            var connectionString = configuration.ResolveConnectionString("App_Database");

            // Assert
            connectionString.Should().Be("Server=server1;Database=db1;User Id=web;Password=pass;");
        }

        [Fact]
        public void Should_prefer_segments_over_string_value()
        {
            // Arrange
            var segments = LegacySegments();
            segments.Add("ConnectionStrings:App_Database", "Server=server0;Database=db0;User Id=a;Password=a");
            var configuration = BuildConfiguration(segments);

            // Act
            var connectionString = configuration.ResolveConnectionString("App_Database");

            // Assert
            connectionString.Should().Be("Server=server1;Database=db1;User Id=web;Password=pass;MultipleActiveResultSets=true");
        }

        [Theory]
        [InlineData("Server")]
        [InlineData("Database")]
        [InlineData("UserName")]
        [InlineData("Password")]
        public void Should_throw_when_segment_is_missing(string segment)
        {
            // Arrange
            var segments = LegacySegments();
            segments[$"ConnectionStrings:App_Database:{segment}"] = " ";
            var configuration = BuildConfiguration(segments);

            // Act
            Action act = () => configuration.ResolveConnectionString("App_Database");

            // Assert
            act.Should().Throw<ConnectionStringConfigurationException>().WithMessage($"Connection string part {segment} is not provided!");
        }

        [Fact]
        public void Should_ignore_whitespace_authentication()
        {
            // Arrange
            var segments = LegacySegments();
            segments.Add("ConnectionStrings:App_Database:Authentication", "   ");
            segments.Add("ConnectionStrings:App_Database:IdentityClientId", IdentityClientId);
            var configuration = BuildConfiguration(segments);

            // Act
            var connectionString = configuration.ResolveConnectionString("App_Database");

            // Assert
            connectionString.Should().Be("Server=server1;Database=db1;User Id=web;Password=pass;MultipleActiveResultSets=true");
        }

        [Fact]
        public void Should_build_workload_identity_ignoring_credentials()
        {
            // Arrange
            var configuration = BuildConfiguration(IdentitySegments("Active Directory Workload Identity"));

            // Act
            var connectionString = configuration.ResolveConnectionString("App_Database");

            // Assert
            connectionString.Should().Be($"Server=server1;Database=db1;User Id={IdentityClientId};Authentication=Active Directory Workload Identity;MultipleActiveResultSets=true");
        }

        [Fact]
        public void Should_build_workload_identity_without_other_params()
        {
            // Arrange
            var segments = IdentitySegments("Active Directory Workload Identity");
            segments.Remove("ConnectionStrings:App_Database:OtherParams");
            var configuration = BuildConfiguration(segments);

            // Act
            var connectionString = configuration.ResolveConnectionString("App_Database");

            // Assert
            connectionString.Should().Be($"Server=server1;Database=db1;User Id={IdentityClientId};Authentication=Active Directory Workload Identity;");
        }

        [Theory]
        [InlineData("Active Directory Workload Identity")]
        [InlineData("ActiveDirectoryWorkloadIdentity")]
        [InlineData("active directory workload identity")]
        [InlineData("ACTIVEDIRECTORYWORKLOADIDENTITY")]
        [InlineData("Active  Directory Workload  Identity")]
        [InlineData("Active Directory Managed Identity")]
        [InlineData("ActiveDirectoryManagedIdentity")]
        [InlineData("Active Directory MSI")]
        [InlineData("ActiveDirectoryMSI")]
        [InlineData("Active Directory Default")]
        [InlineData("activedirectorydefault")]
        public void Should_build_passwordless_authentication_modes(string authentication)
        {
            // Arrange
            var configuration = BuildConfiguration(IdentitySegments(authentication));

            // Act
            var connectionString = configuration.ResolveConnectionString("App_Database");

            // Assert
            connectionString.Should().Be($"Server=server1;Database=db1;User Id={IdentityClientId};Authentication={authentication};MultipleActiveResultSets=true");
        }

        [Theory]
        [InlineData("Server")]
        [InlineData("Database")]
        [InlineData("IdentityClientId")]
        public void Should_throw_when_passwordless_segment_is_missing(string segment)
        {
            // Arrange
            var segments = IdentitySegments("Active Directory Workload Identity");
            segments.Remove($"ConnectionStrings:App_Database:{segment}");
            var configuration = BuildConfiguration(segments);

            // Act
            Action act = () => configuration.ResolveConnectionString("App_Database");

            // Assert
            act.Should().Throw<ConnectionStringConfigurationException>().WithMessage($"Connection string part {segment} is not provided!");
        }

        [Fact]
        public void Should_build_service_principal_with_credentials()
        {
            // Arrange
            var configuration = BuildConfiguration(IdentitySegments("Active Directory Service Principal"));

            // Act
            var connectionString = configuration.ResolveConnectionString("App_Database");

            // Assert
            connectionString.Should().Be("Server=server1;Database=db1;User Id=web;Password=pass;Authentication=Active Directory Service Principal;MultipleActiveResultSets=true");
        }

        [Fact]
        public void Should_throw_when_service_principal_password_is_missing()
        {
            // Arrange
            var segments = IdentitySegments("Active Directory Service Principal");
            segments.Remove("ConnectionStrings:App_Database:Password");
            var configuration = BuildConfiguration(segments);

            // Act
            Action act = () => configuration.ResolveConnectionString("App_Database");

            // Assert
            act.Should().Throw<ConnectionStringConfigurationException>().WithMessage("Connection string part Password is not provided!");
        }

        [Fact]
        public void Should_compose_segments_from_multiple_sources()
        {
            // Arrange
            var prefix = $"NBB_{Guid.NewGuid():N}_";
            Environment.SetEnvironmentVariable($"{prefix}ConnectionStrings__App_Database__Authentication", "Active Directory Workload Identity");
            try
            {
                var configuration = new ConfigurationBuilder()
                    // e.g. config map
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "ConnectionStrings:App_Database:Server", "server1" },
                        { "ConnectionStrings:App_Database:Database", "db1" }
                    })
                    // e.g. secret
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "ConnectionStrings:App_Database:UserName", "web" },
                        { "ConnectionStrings:App_Database:Password", "pass" },
                        { "ConnectionStrings:App_Database:IdentityClientId", IdentityClientId }
                    })
                    // e.g. pod environment
                    .AddEnvironmentVariables(prefix)
                    .Build();

                // Act
                var connectionString = configuration.ResolveConnectionString("App_Database");

                // Assert
                connectionString.Should().Be($"Server=server1;Database=db1;User Id={IdentityClientId};Authentication=Active Directory Workload Identity;");
            }
            finally
            {
                Environment.SetEnvironmentVariable($"{prefix}ConnectionStrings__App_Database__Authentication", null);
            }
        }

        [Fact]
        public void Should_build_workload_identity_parsable_by_sql_client()
        {
            // Arrange
            var configuration = BuildConfiguration(IdentitySegments("Active Directory Workload Identity"));

            // Act
            var builder = new SqlConnectionStringBuilder(configuration.ResolveConnectionString("App_Database"));

            // Assert
            builder.Authentication.Should().Be(SqlAuthenticationMethod.ActiveDirectoryWorkloadIdentity);
            builder.UserID.Should().Be(IdentityClientId);
            builder.Password.Should().BeEmpty();
            builder.DataSource.Should().Be("server1");
            builder.InitialCatalog.Should().Be("db1");
            builder.MultipleActiveResultSets.Should().BeTrue();
        }

        private static Dictionary<string, string?> LegacySegments() => new()
        {
            { "ConnectionStrings:App_Database:Server", "server1" },
            { "ConnectionStrings:App_Database:Database", "db1" },
            { "ConnectionStrings:App_Database:UserName", "web" },
            { "ConnectionStrings:App_Database:Password", "pass" },
            { "ConnectionStrings:App_Database:OtherParams", "MultipleActiveResultSets=true" }
        };

        private static Dictionary<string, string?> IdentitySegments(string authentication)
        {
            var segments = LegacySegments();
            segments.Add("ConnectionStrings:App_Database:Authentication", authentication);
            segments.Add("ConnectionStrings:App_Database:IdentityClientId", IdentityClientId);
            return segments;
        }

        private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
            => new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
