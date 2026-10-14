// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Microsoft.Data.SqlClient;
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
using static NBB.MultiTenancy.Abstractions.Configuration.TenantConfigurationExtensions;

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
                                ""Password"": ""pass"",
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
            tenantConfig.GetConnectionString("Leasing_Database").Should().Be("Server=server1;Database=db1;User Id=web;Password=pass;MultipleActiveResultSets=true");
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
                              ""UserName"": ""web"",
                              ""Password"": ""pass""
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
            tenantConfig.GetConnectionString("Leasing_Database").Should().Be("Server=cf-erp17\\mama;Database=db1;User Id=web;Password=pass;");
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
                                ""Password"": ""pass"",
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
            tenantConfig.GetConnectionString("Leasing_Database").Should().Be("Server=server1;Database=db1;User Id=web;Password=pass;MultipleActiveResultSets=true");
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

        [Fact]
        public void load_connectionString_splitted_tenant1_missing_database()
        {
            // Arrange
            var myConfiguration = @"{
                      ""MultiTenancy"": {
                        ""Tenants"": {
                          ""MBFS"": {
                            ""TenantId"": ""68a448a2-e7d8-4875-8127-f18668217eb6"",
                            ""ConnectionStrings"": {
                              ""Leasing_Database"": {
                                ""Server"": ""server1"",
                                ""Database"": """",
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

            tca.TenantContext = new TenantContext(new Tenant(tid, "MBFS"));
            var tenantConfig = new TenantConfiguration(configuration,
                new OptionsWrapper<TenancyHostingOptions>(new TenancyHostingOptions()
                    { TenancyType = TenancyType.MultiTenant }), tca);

            // Act
            Action act = () => tenantConfig.GetConnectionString("Leasing_Database");

            // Assert
            act.Should().Throw<TenantConnectionStringBuilderException>().WithMessage("Connection string part Database is not provided!");
        }

        private const string MbfsTenantId = "68a448a2-e7d8-4875-8127-f18668217eb6";
        private const string BcrTenantId = "ef8d5362-9969-4e02-8794-0d1af56816f6";
        private const string IdentityClientId = "3f2c1a9e-6b7d-4e8f-9a0b-1c2d3e4f5a6b";

        [Fact]
        public void load_connectionString_workload_identity_ignores_default_credentials()
        {
            // Arrange
            var tenantConfig = GetTenantConfiguration(GetIdentityConfigJson("Active Directory Workload Identity"), MbfsTenantId, "MBFS");

            // Act
            var connectionString = tenantConfig.GetConnectionString("Leasing_Database");

            // Assert
            connectionString.Should().Be($"Server=server1;Database=db1;User Id={IdentityClientId};Authentication=Active Directory Workload Identity;MultipleActiveResultSets=true");
        }

        [Fact]
        public void load_connectionString_workload_identity_without_other_params()
        {
            // Arrange
            var myConfiguration = @"{
                      ""MultiTenancy"": {
                        ""Tenants"": {
                          ""MBFS"": {
                            ""TenantId"": ""68a448a2-e7d8-4875-8127-f18668217eb6"",
                            ""ConnectionStrings"": {
                              ""Leasing_Database"": {
                                ""Server"": ""server1"",
                                ""Database"": ""db1"",
                                ""Authentication"": ""Active Directory Workload Identity"",
                                ""IdentityClientId"": """ + IdentityClientId + @"""
                              }
                            }
                          }
                        }
                      }
                    }";
            var tenantConfig = GetTenantConfiguration(myConfiguration, MbfsTenantId, "MBFS");

            // Act
            var connectionString = tenantConfig.GetConnectionString("Leasing_Database");

            // Assert
            connectionString.Should().Be($"Server=server1;Database=db1;User Id={IdentityClientId};Authentication=Active Directory Workload Identity;");
        }

        [Fact]
        public void load_connectionString_workload_identity_missing_identity_client_id()
        {
            // Arrange
            var myConfiguration = @"{
                      ""MultiTenancy"": {
                        ""Defaults"": {
                          ""ConnectionStrings"": {
                            ""Leasing_Database"": {
                              ""UserName"": ""web"",
                              ""Password"": ""pass""
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
                                ""Authentication"": ""Active Directory Workload Identity""
                              }
                            }
                          }
                        }
                      }
                    }";
            var tenantConfig = GetTenantConfiguration(myConfiguration, MbfsTenantId, "MBFS");

            // Act
            Action act = () => tenantConfig.GetConnectionString("Leasing_Database");

            // Assert
            act.Should().Throw<TenantConnectionStringBuilderException>().WithMessage("Connection string part IdentityClientId is not provided!");
        }

        [Fact]
        public void load_connectionString_workload_identity_missing_server()
        {
            // Arrange
            var myConfiguration = @"{
                      ""MultiTenancy"": {
                        ""Tenants"": {
                          ""MBFS"": {
                            ""TenantId"": ""68a448a2-e7d8-4875-8127-f18668217eb6"",
                            ""ConnectionStrings"": {
                              ""Leasing_Database"": {
                                ""Database"": ""db1"",
                                ""Authentication"": ""Active Directory Workload Identity"",
                                ""IdentityClientId"": """ + IdentityClientId + @"""
                              }
                            }
                          }
                        }
                      }
                    }";
            var tenantConfig = GetTenantConfiguration(myConfiguration, MbfsTenantId, "MBFS");

            // Act
            Action act = () => tenantConfig.GetConnectionString("Leasing_Database");

            // Assert
            act.Should().Throw<TenantConnectionStringBuilderException>().WithMessage("Connection string part Server is not provided!");
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
        public void load_connectionString_passwordless_authentication_modes(string authentication)
        {
            // Arrange
            var tenantConfig = GetTenantConfiguration(GetIdentityConfigJson(authentication), MbfsTenantId, "MBFS");

            // Act
            var connectionString = tenantConfig.GetConnectionString("Leasing_Database");

            // Assert
            connectionString.Should().Be($"Server=server1;Database=db1;User Id={IdentityClientId};Authentication={authentication};MultipleActiveResultSets=true");
        }

        [Fact]
        public void load_connectionString_service_principal_keeps_credentials()
        {
            // Arrange
            var tenantConfig = GetTenantConfiguration(GetIdentityConfigJson("Active Directory Service Principal"), MbfsTenantId, "MBFS");

            // Act
            var connectionString = tenantConfig.GetConnectionString("Leasing_Database");

            // Assert
            connectionString.Should().Be("Server=server1;Database=db1;User Id=web;Password=pass;Authentication=Active Directory Service Principal;MultipleActiveResultSets=true");
        }

        [Fact]
        public void load_connectionString_service_principal_missing_password()
        {
            // Arrange
            var myConfiguration = @"{
                      ""MultiTenancy"": {
                        ""Tenants"": {
                          ""MBFS"": {
                            ""TenantId"": ""68a448a2-e7d8-4875-8127-f18668217eb6"",
                            ""ConnectionStrings"": {
                              ""Leasing_Database"": {
                                ""Server"": ""server1"",
                                ""Database"": ""db1"",
                                ""UserName"": ""web"",
                                ""Authentication"": ""Active Directory Service Principal""
                              }
                            }
                          }
                        }
                      }
                    }";
            var tenantConfig = GetTenantConfiguration(myConfiguration, MbfsTenantId, "MBFS");

            // Act
            Action act = () => tenantConfig.GetConnectionString("Leasing_Database");

            // Assert
            act.Should().Throw<TenantConnectionStringBuilderException>().WithMessage("Connection string part Password is not provided!");
        }

        [Fact]
        public void load_connectionString_whitespace_authentication_uses_legacy_format()
        {
            // Arrange
            var tenantConfig = GetTenantConfiguration(GetIdentityConfigJson("   "), MbfsTenantId, "MBFS");

            // Act
            var connectionString = tenantConfig.GetConnectionString("Leasing_Database");

            // Assert
            connectionString.Should().Be("Server=server1;Database=db1;User Id=web;Password=pass;MultipleActiveResultSets=true");
        }

        [Fact]
        public void load_connectionString_workload_identity_only_for_one_tenant()
        {
            // Arrange
            var configJson = GetIdentityConfigJson("Active Directory Workload Identity");

            // Act (the tenant context is ambient, so resolve one tenant at a time)
            var mbfsConnectionString = GetTenantConfiguration(configJson, MbfsTenantId, "MBFS").GetConnectionString("Leasing_Database");
            var bcrConnectionString = GetTenantConfiguration(configJson, BcrTenantId, "BCR").GetConnectionString("Leasing_Database");

            // Assert
            mbfsConnectionString.Should().Be($"Server=server1;Database=db1;User Id={IdentityClientId};Authentication=Active Directory Workload Identity;MultipleActiveResultSets=true");
            bcrConnectionString.Should().Be("Server=server2;Database=db2;User Id=web;Password=pass;");
        }

        [Fact]
        public void workload_identity_connectionString_is_parsed_by_sql_client()
        {
            // Arrange
            var tenantConfig = GetTenantConfiguration(GetIdentityConfigJson("Active Directory Workload Identity"), MbfsTenantId, "MBFS");

            // Act
            var builder = new SqlConnectionStringBuilder(tenantConfig.GetConnectionString("Leasing_Database"));

            // Assert
            builder.Authentication.Should().Be(SqlAuthenticationMethod.ActiveDirectoryWorkloadIdentity);
            builder.UserID.Should().Be(IdentityClientId);
            builder.Password.Should().BeEmpty();
            builder.DataSource.Should().Be("server1");
            builder.InitialCatalog.Should().Be("db1");
            builder.MultipleActiveResultSets.Should().BeTrue();
        }

        // MBFS overrides Authentication and IdentityClientId; UserName/Password come from Defaults; BCR keeps the legacy configuration
        private static string GetIdentityConfigJson(string authentication) => @"{
                      ""MultiTenancy"": {
                        ""Defaults"": {
                          ""ConnectionStrings"": {
                            ""Leasing_Database"": {
                              ""UserName"": ""web"",
                              ""Password"": ""pass""
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
                                ""OtherParams"": ""MultipleActiveResultSets=true"",
                                ""Authentication"": """ + authentication + @""",
                                ""IdentityClientId"": """ + IdentityClientId + @"""
                              }
                            }
                          },
                          ""BCR"": {
                            ""TenantId"": ""ef8d5362-9969-4e02-8794-0d1af56816f6"",
                            ""ConnectionStrings"": {
                              ""Leasing_Database"": {
                                ""Server"": ""server2"",
                                ""Database"": ""db2""
                              }
                            }
                          }
                        }
                      }
                    }";

        private static TenantConfiguration GetTenantConfiguration(string json, string tenantId, string tenantCode)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
                .Build();

            var tca = new TenantContextAccessor
            {
                TenantContext = new TenantContext(new Tenant(Guid.Parse(tenantId), tenantCode))
            };

            return new TenantConfiguration(configuration,
                new OptionsWrapper<TenancyHostingOptions>(new TenancyHostingOptions()
                    { TenancyType = TenancyType.MultiTenant }), tca);
        }
    }
}
