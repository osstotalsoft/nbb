// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NBB.Core.Abstractions;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Configuration;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;
using Xunit;

namespace NBB.Data.EntityFramework.MultiTenancy.Tests
{
    public class MultiTenancyTests
    {
        [Fact]
        public async Task Should_add_tenantId()
        {
            // arrange
            var testTenantId = Guid.NewGuid();
            var sp = GetServiceProvider<TestDbContext>(true);
            var testEntity = new TestEntity { Id = 1 };

            await WithTenantScope(sp, testTenantId, async sp =>
            {
                var dbContext = sp.GetRequiredService<TestDbContext>();
                dbContext.TestEntities.Add(testEntity);
                var uow = sp.GetRequiredService<IUow<TestEntity>>();

                // act
                await uow.SaveChangesAsync();

                // assert
                dbContext.Entry(testEntity).GetTenantId().Should().Be(testTenantId);
            });
        }

        [Fact]
        public async Task Should_Exception_Be_Thrown_If_Different_TenantIds()
        {
            // arrange
            var testTenantId = Guid.NewGuid();
            var sp = GetServiceProvider<TestDbContext>(true);
            var testEntity = new TestEntity { Id = 1 };
            var testEntityOtherId = new TestEntity { Id = 2 };
            await WithTenantScope(sp, testTenantId, async sp =>
            {
                var dbContext = sp.GetRequiredService<TestDbContext>();
                var uow = sp.GetRequiredService<IUow<TestEntity>>();

                dbContext.TestEntities.Add(testEntity);
                dbContext.TestEntities.Add(testEntityOtherId);

                await uow.SaveChangesAsync(); // Bypasses multi tenancy UoW !
                dbContext.Entry(testEntityOtherId).Property("TenantId").CurrentValue = Guid.NewGuid();

                // act && assert
                Exception ex = Assert.Throws<Exception>(() => uow.SaveChangesAsync().GetAwaiter().GetResult());
            });
        }

        [Fact]
        public async Task Shoud_Apply_Filter()
        {
            // arrange
            var testTenantId1 = Guid.NewGuid();
            var testTenantId2 = Guid.NewGuid();
            var testEntity = new TestEntity { Id = 1 };
            var testEntityOtherId = new TestEntity { Id = 2 };
            var sp = GetServiceProvider<TestDbContext>(true);

            await WithTenantScope(sp, testTenantId1, async sp =>
            {
                var dbContext = sp.GetRequiredService<TestDbContext>();
                dbContext.TestEntities.Add(testEntity);
                await dbContext.SaveChangesAsync();
            });

            await WithTenantScope(sp, testTenantId2, async sp =>
            {
                var dbContext = sp.GetRequiredService<TestDbContext>();
                dbContext.TestEntities.Add(testEntityOtherId);
                await dbContext.SaveChangesAsync();
            });

            await WithTenantScope(sp, testTenantId2, async sp =>
            {
                var dbContext = sp.GetRequiredService<TestDbContext>();

                // act
                var entities = await dbContext.TestEntities.ToListAsync();

                // assert
                entities.Count.Should().Be(1);
            });
        }


        [Fact]
        public async Task Should_add_TenantId_and_filter_for_MultiTenantContext()
        {
            // arrange
            var testTenantId = Guid.NewGuid();
            var sp = GetServiceProvider<TestDbContext>(true);
            var testEntity = new TestEntity { Id = 1 };
            var testEntity1 = new TestEntity { Id = 2 };

            await WithTenantScope(sp, testTenantId, async sp =>
            {
                var dbContext = sp.GetRequiredService<TestDbContext>();

                dbContext.TestEntities.Add(testEntity);
                dbContext.TestEntities.Add(testEntity1);

                // act
                await dbContext.SaveChangesAsync();
                var list = dbContext
                    .TestEntities
                    .Select(x => (Guid)dbContext.Entry(x).Property("TenantId").CurrentValue)
                    .ToList()
                    .Distinct();

                // assert
                list.Count().Should().Be(1);
            });
        }

        private IServiceProvider GetServiceProvider<TDBContext>(bool isSharedDB) where TDBContext : DbContext
        {
            var tenantService = Mock.Of<ITenantContextAccessor>(x => x.TenantContext == null);
            var tenantDatabaseConfigService =
                Mock.Of<ITenantConfiguration>(x => x.GetValue<string>(It.Is<string>(a=>a.Contains("ConnectionString")))
                                                   == (isSharedDB ? "Test" : Guid.NewGuid().ToString()));
            var services = new ServiceCollection();
            services.AddOptions<TenancyHostingOptions>().Configure(o =>
            {
                o.TenancyType = TenancyType.MultiTenant;
            });
            services.AddSingleton(tenantService);
            services.AddSingleton(tenantDatabaseConfigService);
            services.AddLogging();

            services.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<TDBContext>((sp, options) =>
                {
                    var conn = sp.GetRequiredService<ITenantConfiguration>().GetConnectionString();
                    options.UseInMemoryDatabase(conn).UseInternalServiceProvider(sp);
                });

            services.AddEfUow<TestEntity, TDBContext>();

            return services.BuildServiceProvider();
        }

        private static async Task WithTenantScope(IServiceProvider sp, Guid tenantId, Func<IServiceProvider, Task> action)
        {
            var scope = sp.CreateScope();
            var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            tenantContextAccessor.TenantContext = new TenantContext(new Tenant(tenantId, string.Empty));

            await action(scope.ServiceProvider);
        }
    }
}
