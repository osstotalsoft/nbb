// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Configuration;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace NBB.MultiTenancy.Abstractions.Tests
{
    public class TenantConfigurationTests
    {
        [Fact]
        public void load_value_from_config()
        {
            // Arrange
            var myConfiguration = new Dictionary<string, string>
            {
                {"ConnectionStrings:ConnectionString1", "ConnectionStringT0"},
                {"OtherProp1", "OtherProp1"},
                {"MultiTenancy:Defaults:ConnectionStrings:ConnectionString1", "ConnectionStringDefault"},
                {"MultiTenancy:Defaults:OtherProp2", "OtherProp2"},
                {"MultiTenancy:Tenants:MBFS:TenantId", "68a448a2-e7d8-4875-8127-f18668217eb6"},
                {"MultiTenancy:Tenants:MBFS:ConnectionStrings:ConnectionString1", "ConnectionStringT1"},
                {"MultiTenancy:Tenants:BCR:TenantId", "da84628a-2925-4b69-9116-a90dd5a72b1f"},
                {"MultiTenancy:Tenants:BCR:ConnectionStrings:ConnectionString1", "ConnectionStringT2"},
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            var tid = Guid.Parse("68a448a2-e7d8-4875-8127-f18668217eb6");
            var tca = new TenantContextAccessor();

            //arrange
            tca.TenantContext = new TenantContext(new Tenant(tid,"MBFS"));
            var tenantConfig = new TenantConfiguration(configuration,
                new OptionsWrapper<TenancyHostingOptions>(new TenancyHostingOptions()
                    { TenancyType = TenancyType.MultiTenant }), tca);

            // Assert
            tenantConfig.GetConnectionString("ConnectionString_").Should().BeNull();
            tenantConfig.GetConnectionString("ConnectionString1").Should().Be("ConnectionStringT1");
            tenantConfig.GetValue<string>("TenantId").Should().Be("68a448a2-e7d8-4875-8127-f18668217eb6");
            tenantConfig.GetValue<string>("OtherProp1").Should().BeNull();
            tenantConfig.GetValue<string>("OtherProp2").Should().Be("OtherProp2");
            
        }

        [Fact]
        public void load_value_from_defaults()
        {
            // Arrange
            var myConfiguration = new Dictionary<string, string>
            {
                {"ConnectionStrings:ConnectionString1", "ConnectionStringT0"},
                {"OtherProp1", "OtherProp1"},
                {"MultiTenancy:Defaults:ConnectionStrings:ConnectionString1", "ConnectionStringDefault"},
                {"MultiTenancy:Defaults:OtherProp2", "OtherProp2"},
                {"MultiTenancy:Tenants:MBFS:TenantId", "68a448a2-e7d8-4875-8127-f18668217eb6"},
                {"MultiTenancy:Tenants:MBFS:ConnectionStrings:ConnectionString1", "ConnectionStringT1"},
                {"MultiTenancy:Tenants:BCR:TenantId", "da84628a-2925-4b69-9116-a90dd5a72b1f"},
                {"MultiTenancy:Tenants:BCR:ConnectionStrings:ConnectionString1", "ConnectionStringT2"},
                {"MultiTenancy:Tenants:BRD:TenantId", "4ec0398f-4335-41ca-a3be-d2f487d5d568"},
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            var tid = Guid.Parse("4ec0398f-4335-41ca-a3be-d2f487d5d568");
            var tca = new TenantContextAccessor();

            //arrange
            tca.TenantContext = new TenantContext(new Tenant(tid, "BRD"));
            var tenantConfig = new TenantConfiguration(configuration,
                new OptionsWrapper<TenancyHostingOptions>(new TenancyHostingOptions()
                    { TenancyType = TenancyType.MultiTenant }), tca);

            // Assert
            tenantConfig.GetConnectionString("ConnectionString_").Should().BeNull();
            tenantConfig.GetConnectionString("ConnectionString1").Should().Be("ConnectionStringDefault");
            tenantConfig.GetValue<string>("TenantId").Should().Be("4ec0398f-4335-41ca-a3be-d2f487d5d568");
            tenantConfig.GetValue<string>("OtherProp1").Should().BeNull();
            tenantConfig.GetValue<string>("OtherProp2").Should().Be("OtherProp2");
        }

        [Fact]
        public void load_value_from_config_monotenant()
        {
            // Arrange
            var myConfiguration = new Dictionary<string, string>
            {
                {"ConnectionStrings:ConnectionString1", "ConnectionStringT0"},
                {"OtherProp1", "OtherProp1"},
                {"MultiTenancy:Defaults:ConnectionStrings:ConnectionString1", "ConnectionStringDefault"},
                {"MultiTenancy:Defaults:OtherProp2", "OtherProp2"},
                {"MultiTenancy:Tenants:MBFS:TenantId", "68a448a2-e7d8-4875-8127-f18668217eb6"},
                {"MultiTenancy:Tenants:MBFS:ConnectionStrings:ConnectionString1", "ConnectionStringT1"},
                {"MultiTenancy:Tenants:BCR:TenantId", "da84628a-2925-4b69-9116-a90dd5a72b1f"},
                {"MultiTenancy:Tenants:BCR:ConnectionStrings:ConnectionString1", "ConnectionStringT2"},
                {"MultiTenancy:Tenants:BRD:TenantId", "4ec0398f-4335-41ca-a3be-d2f487d5d568"},
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            var tid = Guid.Parse("4ec0398f-4335-41ca-a3be-d2f487d5d568");
            var tca = new TenantContextAccessor();

            //arrange
            var tenantConfig = new TenantConfiguration(configuration,
                new OptionsWrapper<TenancyHostingOptions>(new TenancyHostingOptions()
                    { TenancyType = TenancyType.MonoTenant }), tca);

            // Assert
            tenantConfig.GetConnectionString("ConnectionString_").Should().BeNull();
            tenantConfig.GetConnectionString("ConnectionString1").Should().Be("ConnectionStringT0");
            tenantConfig.GetValue<string>("TenantId").Should().BeNull();
            tenantConfig.GetValue<string>("OtherProp1").Should().Be("OtherProp1");
            tenantConfig.GetValue<string>("OtherProp2").Should().BeNull();
        }


        [Fact]
        public void load_connectionString_splitted_tenant1()
        {
            // Arrange
            var myConfiguration = @"{
                      ""MultiTenancy"": {
                        ""Defaults"": {
                          ""ConnectionStrings"": {
                            ""Leasing_Database"": {
                              ""Server"": ""cf-erp17"",
                              ""Database"": ""db0"",
                              ""UserName"": ""web"",
                              ""Password"": ""mama123""
                            }
                          }
                        },
                        ""Tenants"": {
                          ""MBFS"": {
                            ""TenantId"": ""68a448a2-e7d8-4875-8127-f18668217eb6"",
                            ""ConnectionStrings"": {
                              ""Leasing_Database"": {
                                ""Server"": ""server1"",
                                ""Database"": ""db1"",
                                ""UserName"": ""web"",
                                ""OtherParams"": ""MultipleActiveResultSets=true""
                              }
                            }
                          },
                          ""BCR"": {
                            ""TenantId"": ""ef8d5362-9969-4e02-8794-0d1af56816f6"",
                            ""Code"": ""BCR""
                          },
                          ""DEV"": {
                            ""TenantId"": ""da84628a-2925-4b69-9116-a90dd5a72b1f"",
                            ""Code"": ""DEV""
                          }
                        }
                      }
                    }";

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(myConfiguration)))
                .Build();

            var tid = Guid.Parse("68a448a2-e7d8-4875-8127-f18668217eb6");
            var tca = new TenantContextAccessor();

            //arrange
            tca.TenantContext = new TenantContext(new Tenant(tid, "MBFS"));
            var tenantConfig = new TenantConfiguration(configuration,
                new OptionsWrapper<TenancyHostingOptions>(new TenancyHostingOptions()
                    { TenancyType = TenancyType.MultiTenant }), tca);


            // Assert
            tenantConfig.GetConnectionString("ConnectionString_").Should().BeNull();
            tenantConfig.GetConnectionString("Leasing_Database").Should().Be("Server=server1;Database=db1;User Id=web;Password=mama123;MultipleActiveResultSets=true");
            tenantConfig.GetValue<string>("TenantId").Should().Be("68a448a2-e7d8-4875-8127-f18668217eb6");
        }

        [Fact]
        public void load_connectionString_splitted_without_defaults()
        {
            // Arrange
            var myConfiguration = @"{
                      ""MultiTenancy"": {
                        ""Defaults"": {
                          ""ConnectionStrings"": { }
                        },
                        ""Tenants"": {
                          ""MBFS"": {
                            ""TenantId"": ""68a448a2-e7d8-4875-8127-f18668217eb6"",
                            ""ConnectionStrings"": {
                              ""Leasing_Database"": {
                                ""Server"": ""server1"",
                                ""Database"": ""db1"",
                                ""UserName"": ""web"",
                                ""OtherParams"": ""MultipleActiveResultSets=true""
                              }
                            }
                          },
                          ""BCR"": {
                            ""TenantId"": ""ef8d5362-9969-4e02-8794-0d1af56816f6"",
                            ""Code"": ""BCR""
                          },
                          ""DEV"": {
                            ""TenantId"": ""da84628a-2925-4b69-9116-a90dd5a72b1f"",
                            ""Code"": ""DEV""
                          }
                        }
                      }
                    }";

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(myConfiguration)))
                .Build();

            var tid = Guid.Parse("68a448a2-e7d8-4875-8127-f18668217eb6");
            var tca = new TenantContextAccessor();

            //arrange
            tca.TenantContext = new TenantContext(new Tenant(tid, "MBFS"));
            var tenantConfig = new TenantConfiguration(configuration,
                new OptionsWrapper<TenancyHostingOptions>(new TenancyHostingOptions()
                { TenancyType = TenancyType.MultiTenant }), tca);


            // Assert
            tenantConfig.GetConnectionString("ConnectionString_").Should().BeNull();
            tenantConfig.GetConnectionString("Leasing_Database").Should().Be("Server=server1;Database=db1;User Id=web;Password=;MultipleActiveResultSets=true");
            tenantConfig.GetValue<string>("TenantId").Should().Be("68a448a2-e7d8-4875-8127-f18668217eb6");
        }


        [Fact]
        public void load_connectionString_splitted_from_default()
        {
            // Arrange
            var myConfiguration = @"{
                      ""MultiTenancy"": {
                        ""Defaults"": {
                          ""ConnectionStrings"": {
                            ""Leasing_Database"": {
                              ""Server"": ""cf-erp17\\mama"",
                              ""Database"": ""db1"",
                              ""UserName"": ""web""
                            }
                          }
                        },
                        ""Tenants"": {
                          ""MBFS"": {
                            ""TenantId"": ""68a448a2-e7d8-4875-8127-f18668217eb6"",
                            ""ConnectionStrings"": {
                              ""Leasing_Database"": {
                                ""Server"": ""server1"",
                                ""Database"": ""db1"",
                                ""UserName"": ""web"",
                                ""OtherParams"": ""MultipleActiveResultSets=true""
                              }
                            }
                          },
                          ""BCR"": {
                            ""TenantId"": ""ef8d5362-9969-4e02-8794-0d1af56816f6"",
                            ""Code"": ""BCR""
                          },
                          ""DEV"": {
                            ""TenantId"": ""da84628a-2925-4b69-9116-a90dd5a72b1f"",
                            ""Code"": ""DEV""
                          }
                        }
                      }
                    }";

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(myConfiguration)))
                .Build();

            var tid = Guid.Parse("ef8d5362-9969-4e02-8794-0d1af56816f6");
            var tca = new TenantContextAccessor();

            //arrange
            tca.TenantContext = new TenantContext(new Tenant(tid, "BCR"));
            var tenantConfig = new TenantConfiguration(configuration,
                new OptionsWrapper<TenancyHostingOptions>(new TenancyHostingOptions()
                { TenancyType = TenancyType.MultiTenant }), tca);


            // Assert
            tenantConfig.GetConnectionString("ConnectionString_").Should().BeNull();
            tenantConfig.GetConnectionString("Leasing_Database").Should().Be("Server=cf-erp17\\mama;Database=db1;User Id=web;Password=;");
            tenantConfig.GetValue<string>("TenantId").Should().Be("ef8d5362-9969-4e02-8794-0d1af56816f6");
        }

        [Fact]
        public void load_connectionString_concatenated_from_default()
        {
            // Arrange
            var myConfiguration = @"{
                      ""MultiTenancy"": {
                        ""Defaults"": {
                          ""ConnectionStrings"": {
                            ""Leasing_Database"": ""Server=server0;Database=lsngdbqa;User Id=web;Password=pas;MultipleActiveResultSets=true""
                          }
                        },
                        ""Tenants"": {
                          ""MBFS"": {
                            ""TenantId"": ""68a448a2-e7d8-4875-8127-f18668217eb6"",
                            ""ConnectionStrings"": {
                              ""Leasing_Database"": {
                                ""Server"": ""server1"",
                                ""Database"": ""db1"",
                                ""UserName"": ""web"",
                                ""OtherParams"": ""MultipleActiveResultSets=true""
                              }
                            }
                          },
                          ""BCR"": {
                            ""TenantId"": ""ef8d5362-9969-4e02-8794-0d1af56816f6"",
                            ""Code"": ""BCR""
                          },
                          ""DEV"": {
                            ""TenantId"": ""da84628a-2925-4b69-9116-a90dd5a72b1f"",
                            ""Code"": ""DEV""
                          }
                        }
                      }
                    }";

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(myConfiguration)))
                .Build();

            var tid = Guid.Parse("ef8d5362-9969-4e02-8794-0d1af56816f6");
            var tca = new TenantContextAccessor();

            //arrange
            tca.TenantContext = new TenantContext(new Tenant(tid, "BCR"));
            var tenantConfig = new TenantConfiguration(configuration,
                new OptionsWrapper<TenancyHostingOptions>(new TenancyHostingOptions()
                { TenancyType = TenancyType.MultiTenant }), tca);


            // Assert
            tenantConfig.GetConnectionString("ConnectionString_").Should().BeNull();
            tenantConfig.GetConnectionString("Leasing_Database").Should().Be("Server=server0;Database=lsngdbqa;User Id=web;Password=pas;MultipleActiveResultSets=true");
            tenantConfig.GetValue<string>("TenantId").Should().Be("ef8d5362-9969-4e02-8794-0d1af56816f6");
        }


        [Fact]
        public void load_connectionString_with_concatenated_defaults()
        {
            // Arrange
            var myConfiguration = @"{
                      ""MultiTenancy"": {
                        ""Defaults"": {
                          ""ConnectionStrings"": {
                            ""Leasing_Database"": ""Server=server0;Database=db2;User Id=web;Password=pas;MultipleActiveResultSets=true""
                          }
                        },
                        ""Tenants"": {
                          ""MBFS"": {
                            ""TenantId"": ""68a448a2-e7d8-4875-8127-f18668217eb6"",
                            ""ConnectionStrings"": {
                              ""Leasing_Database"": {
                                ""Server"": ""server1"",
                                ""Database"": ""db1"",
                                ""UserName"": ""web"",
                                ""OtherParams"": ""MultipleActiveResultSets=true""
                              }
                            }
                          },
                          ""BCR"": {
                            ""TenantId"": ""ef8d5362-9969-4e02-8794-0d1af56816f6"",
                            ""Code"": ""BCR""
                          },
                          ""BRD"": {
                            ""TenantId"": ""da84628a-2925-4b69-9116-a90dd5a72b1f"",
                            ""Code"": ""DEV""
                          }
                        }
                      }
                    }";

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(myConfiguration)))
                .Build();

            var tid = Guid.Parse("68a448a2-e7d8-4875-8127-f18668217eb6");
            var tca = new TenantContextAccessor();

            //arrange
            tca.TenantContext = new TenantContext(new Tenant(tid, "MBFS"));
            var tenantConfig = new TenantConfiguration(configuration,
                new OptionsWrapper<TenancyHostingOptions>(new TenancyHostingOptions()
                { TenancyType = TenancyType.MultiTenant }), tca);


            // Assert
            tenantConfig.GetConnectionString("ConnectionString_").Should().BeNull();
            tenantConfig.GetConnectionString("Leasing_Database").Should().Be("Server=server1;Database=db1;User Id=web;Password=;MultipleActiveResultSets=true");
            tenantConfig.GetValue<string>("TenantId").Should().Be("68a448a2-e7d8-4875-8127-f18668217eb6");
        }

        [Fact]
        public void load_connectionString_concatenated_with_splitted_defaults()
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
                        ""Tenants"": {
                          ""MBFS"": {
                            ""TenantId"": ""68a448a2-e7d8-4875-8127-f18668217eb6"",
                            ""ConnectionStrings"": {
                              ""Leasing_Database"": ""Server=server0;Database=db;User Id=a;Password=a;MultipleActiveResultSets=true""
                            }
                          },
                          ""BCR"": {
                            ""TenantId"": ""ef8d5362-9969-4e02-8794-0d1af56816f6"",
                            ""Code"": ""BCR""
                          },
                          ""DEV"": {
                            ""TenantId"": ""da84628a-2925-4b69-9116-a90dd5a72b1f"",
                            ""Code"": ""DEV""
                          }
                        }
                      }
                    }";

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(myConfiguration)))
                .Build();

            var tid = Guid.Parse("68a448a2-e7d8-4875-8127-f18668217eb6");
            var tca = new TenantContextAccessor();

            //arrange
            tca.TenantContext = new TenantContext(new Tenant(tid, "MBFS"));
            var tenantConfig = new TenantConfiguration(configuration,
                new OptionsWrapper<TenancyHostingOptions>(new TenancyHostingOptions()
                { TenancyType = TenancyType.MultiTenant }), tca);


            // Assert
            tenantConfig.GetConnectionString("ConnectionString_").Should().BeNull();
            tenantConfig.GetConnectionString("Leasing_Database").Should().Be("Server=server0;Database=db;User Id=a;Password=a;MultipleActiveResultSets=true");
            tenantConfig.GetValue<string>("TenantId").Should().Be("68a448a2-e7d8-4875-8127-f18668217eb6");
        }

        
    }
}
