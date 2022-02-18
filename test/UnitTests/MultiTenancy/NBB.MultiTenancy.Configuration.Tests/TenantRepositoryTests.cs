// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Configuration;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;


namespace NBB.MultiTenancy.Configuration.Tests
{
    public class TenantRepositoryTests
    {
        [Fact]
        public void get_all_tenants_should_work()
        {
            // Arrange
            var myConfiguration = @"{
                      ""MultiTenancy"": {
                        ""Defaults"": {
                          ""ConnectionStrings"": {
                              ""Leasing_Database"": {
                                ""Server"": ""server1"",
                                ""Database"": ""db1"",
                                ""UserName"": ""web"",
                                ""OtherParams"": ""MultipleActiveResultSets=true""
                              }
                          }
                        },
                        ""Tenants"": [
                          {
                            ""TenantId"": ""68a448a2-e7d8-4875-8127-f18668217eb6"",
                            ""ConnectionStrings"": {
                              ""Leasing_Database"": ""Server=server0;Database=lsngdbqa;User Id=web;Password=;MultipleActiveResultSets=true""
                            }
                          },
                          {
                            ""TenantId"": ""ef8d5362-9969-4e02-8794-0d1af56816f6"",
                            ""Code"": ""BCR""
                          },
                          {
                            ""TenantId"": ""da84628a-2925-4b69-9116-a90dd5a72b1f"",
                            ""Code"": ""DEV""
                          }
                        ]
                      }
                    }";

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(myConfiguration)))
                .Build();

            var tenancyHostingOptions = new TenancyHostingOptions()
            {
                TenancyType = TenancyType.MultiTenant
            };

            var options = new OptionsWrapper<TenancyHostingOptions>(tenancyHostingOptions);

            var repo = new ConfigurationTenantRepository(configuration, options);

            //arrange
            var all = repo.GetAll().GetAwaiter().GetResult();

            // Assert
            all.Should().NotBeNull();
            all.Should().NotBeEmpty();
            all.Count.Should().Be(3);

            //all[0].TenantId.Should().Be(Guid.Parse("ef8d5362-9969-4e02-8794-0d1af56816f6"));
            //all[1].TenantId.Should().Be(Guid.Parse("68a448a2-e7d8-4875-8127-f18668217eb6"));
            //all[2].TenantId.Should().Be(Guid.Parse("da84628a-2925-4b69-9116-a90dd5a72b1f"));

            //all[0].Code.Should().Be("BCR");
            //all[1].Code.Should().BeNull();
            //all[2].Code.Should().Be("DEV");
        }
    }
}
