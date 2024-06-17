// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Repositories;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace NBB.MultiTenancy.Abstractions.Tests
{
    public class ConfigurationTenantRepositoryTests
    {
        [Fact]
        public async Task get_all_tenants_should_work()
        {
            // Arrange
            var myConfiguration = """
            {
                "MultiTenancy": {
                    "Defaults": {
                        "ConnectionStrings": {
                            "Leasing_Database": {
                                "Server": "server1",
                                "Database": "db1",
                                "UserName": "web",
                                "OtherParams": "MultipleActiveResultSets=true"
                            }
                        }
                    },
                    "Tenants": {
                        "TNNT1": {
                            "TenantId": "68a448a2-e7d8-4875-8127-f18668217eb6",
                            "ConnectionStrings": {
                                "Leasing_Database": "Server=server0;Database=lsngdbqa;User Id=web;Password=;MultipleActiveResultSets=true"
                            }
                        },
                        "TNNT2": {
                            "TenantId": "ef8d5362-9969-4e02-8794-0d1af56816f6"
                        },
                        "TNNT3": {
                            "TenantId": "da84628a-2925-4b69-9116-a90dd5a72b1f",
                        }
                    }
                }
            }
            """;

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
            var all = await repo.GetAll();

            // Assert
            all.Should().NotBeNull();
            all.Should().NotBeEmpty();
            all.Count.Should().Be(3);
        }

        [Fact]
        public async Task get_all_tenants_should_ignore_not_valid_tenants()
        {
            // Arrange
            var myConfiguration = """
            {
                "MultiTenancy": {
                    "Tenants": {
                        "TNNT1": {
                            "ConnectionStrings": {
                                "Leasing_Database": "Server=server0;Database=lsngdbqa;User Id=web;Password=;MultipleActiveResultSets=true"
                            }
                        },
                    }
                }
            }
            """;

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
            var all = await repo.GetAll();

            // Assert
            all.Should().NotBeNull();
            all.Should().BeEmpty();
        }

        [Fact]
        public async Task get_all_tenants_should_ignore_disabled_tenants()
        {
            // Arrange
            var myConfiguration = """
            {
                "MultiTenancy": {
                    "Tenants": {
                        "TNNT1": {
                            "ConnectionStrings": {
                                "Leasing_Database": "Server=server0;Database=lsngdbqa;User Id=web;Password=;MultipleActiveResultSets=true"
                            },
                            "TenantId": "ef8d5362-9969-4e02-8794-0d1af56816f6",
                            "Enabled": "false"
                        },
                    }
                }
            }
            """;

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
            var all = await repo.GetAll();

            // Assert
            all.Should().NotBeNull();
            all.Should().BeEmpty();
        }

        [Fact]
        public async Task get_tenant_should_throw_for_disabled_tenant()
        {
            // Arrange
            var tenantId = "ef8d5362-9969-4e02-8794-0d1af56816f6";
            var myConfiguration = $$"""
            {
                "MultiTenancy": {
                    "Tenants": {
                        "TNNT1": {
                            "ConnectionStrings": {
                                "Leasing_Database": "Server=server0;Database=lsngdbqa;User Id=web;Password=;MultipleActiveResultSets=true"
                            },
                            "TenantId": "{{tenantId}}",
                            "Enabled": "false"
                        },
                    }
                }
            }
            """;

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(myConfiguration)))
                .Build();

            var tenancyHostingOptions = new TenancyHostingOptions()
            {
                TenancyType = TenancyType.MultiTenant
            };

            var options = new OptionsWrapper<TenancyHostingOptions>(tenancyHostingOptions);

            ITenantRepository repo = new ConfigurationTenantRepository(configuration, options);

            //Act
            Func<Task> action = async() => 
                await repo.Get(Guid.Parse(tenantId));

            //Assert
            await action.Should().ThrowAsync<TenantNotFoundException>();
        }

        [Fact]
        public async Task get_should_bind_tenant_code_from_section_name()
        {
            // Arrange
            var myConfiguration = """
            {
                "MultiTenancy": {
                    "Tenants": {
                        "TNNT1": {
                            "TenantId": "ef8d5362-9969-4e02-8794-0d1af56816f6",
                            "Code": "TNNT2"
                        },
                    }
                }
            }
            """;

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
            var actual = await repo.TryGet(Guid.Parse("ef8d5362-9969-4e02-8794-0d1af56816f6"));

            // Assert
            actual.Should().NotBeNull();
            actual.Code.Should().Be("TNNT1");
        }

        [Fact]
        public async Task get_all_should_bind_tenant_code_from_section_name()
        {
            // Arrange
            var myConfiguration = """
            {
                "MultiTenancy": {
                    "Tenants": {
                        "TNNT1": {
                            "TenantId": "ef8d5362-9969-4e02-8794-0d1af56816f6",
                            "Code": "TNNT2"
                        },
                    }
                }
            }
            """;

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
            var all = await repo.GetAll();

            // Assert
            all.Should().NotBeNull();
            all.Count.Should().Be(1);
            all[0].Code.Should().Be("TNNT1");
        }

    }
}
