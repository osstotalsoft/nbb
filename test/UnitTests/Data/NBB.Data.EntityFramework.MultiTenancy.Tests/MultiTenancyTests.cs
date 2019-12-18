using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NBB.Core.Abstractions;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
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
            var sp = GetServiceProvider<TestDbContext>(testTenantId, true, true);
            var testEntity = new TestEntity { Id = 1 };
            var dbContext = sp.GetRequiredService<TestDbContext>();
            dbContext.TestEntities.Add(testEntity);
            var uow = sp.GetRequiredService<IUow<TestEntity>>();

            // act
            await uow.SaveChangesAsync();

            // assert
            dbContext.Entry(testEntity).GetTenantId().Should().Be(testTenantId);
        }

        [Fact]
        public async Task Should_Exception_Be_Thrown_If_Different_TenantIds()
        {
            // arrange
            var testTenantId = Guid.NewGuid();
            var sp = GetServiceProvider<TestDbContext>(testTenantId, true, true);
            var testEntity = new TestEntity { Id = 1 };
            var testEntityOtherId = new TestEntity { Id = 2 };
            var dbContext = sp.GetRequiredService<TestDbContext>();
            var uow = sp.GetRequiredService<IUow<TestEntity>>();

            dbContext.TestEntities.Add(testEntity);
            dbContext.TestEntities.Add(testEntityOtherId);

            await uow.SaveChangesAsync(); // Bypasses multitenancy UoW !
            dbContext.Entry(testEntityOtherId).Property("TenantId").CurrentValue = Guid.NewGuid();

            // act && assert
            Exception ex = Assert.Throws<Exception>(() => uow.SaveChangesAsync().GetAwaiter().GetResult());
        }

        [Fact]
        public async Task Shoud_Apply_Filter_When_Shared_Database()
        {
            // arrange
            var testTenantId = Guid.NewGuid();
            var sp = GetServiceProvider<TestDbContext>(testTenantId, true, true);
            var testEntity = new TestEntity { Id = 1 };
            var testEntityOtherId = new TestEntity { Id = 2 };
            var dbContext = sp.GetRequiredService<TestDbContext>();

            dbContext.TestEntities.Add(testEntity);
            dbContext.TestEntities.Add(testEntityOtherId);
            dbContext.Entry(testEntity).Property("TenantId").CurrentValue = testTenantId;
            dbContext.Entry(testEntityOtherId).Property("TenantId").CurrentValue = Guid.NewGuid();
            await dbContext.SaveChangesAsync();

            // act
            var list = dbContext.TestEntities.ToList();

            // assert
            list.Count().Should().Be(1);
        }

        [Fact]
        public async Task Shoud_NotApply_Filter_When_NonShared_Database()
        {
            // arrange
            var testTenantId = Guid.NewGuid();
            var sp = GetServiceProvider<TestDbContext>(testTenantId, false, true);
            var testEntity = new TestEntity { Id = 1 };
            var testEntityOtherId = new TestEntity { Id = 2 };
            var dbContext = sp.GetRequiredService<TestDbContext>();

            dbContext.TestEntities.Add(testEntity);
            dbContext.TestEntities.Add(testEntityOtherId);
            dbContext.Entry(testEntity).Property("TenantId").CurrentValue = testTenantId;
            dbContext.Entry(testEntityOtherId).Property("TenantId").CurrentValue = Guid.NewGuid();

            await dbContext.SaveChangesAsync();

            // act
            var list = dbContext.TestEntities.ToList();

            // assert
            list.Count().Should().Be(2);
        }

        [Fact]
        public async Task Should_add_TenantId_and_filter_for_MultiTenantContext()
        {
            // arrange
            var testTenantId = Guid.NewGuid();
            var sp = GetServiceProvider<MultiTenantDbContext>(testTenantId, true, false);
            var testEntity = new TestEntity { Id = 1 };
            var testEntity1 = new TestEntity { Id = 2 };
            var dbContext = sp.GetRequiredService<MultiTenantDbContext>();

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
        }

        private IServiceProvider GetServiceProvider<TDBContext>(Guid tenantId, bool isSharedDB, bool useUow) where TDBContext : DbContext
        {

            var tenantService = Mock.Of<ITenantService>(x =>
                x.GetCurrentTenantAsync() == Task.FromResult(new Tenant(tenantId, "name")));
            var tenantDatabaseConfigService =
                Mock.Of<ITenantDatabaseConfigService>(x => x.IsSharedDatabase(tenantId) == isSharedDB && x.GetConnectionString(tenantId) == "Test");
            var services = new ServiceCollection();
            services.AddSingleton(tenantService);
            services.AddSingleton(tenantDatabaseConfigService);
            services.AddLogging();

            services.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<TDBContext>((sp, options) =>
                {
                    var tenantId = sp.GetRequiredService<ITenantService>().GetCurrentTenantAsync().Result.TenantId;
                    var conn = sp.GetRequiredService<ITenantDatabaseConfigService>().GetConnectionString(tenantId);
                    options.UseInMemoryDatabase(conn).UseInternalServiceProvider(sp);
                });

            if (useUow)
            {
                services.AddMultiTenantEfUow<TestEntity, TDBContext>();
            }

            return services.BuildServiceProvider();
        }
    }
}
