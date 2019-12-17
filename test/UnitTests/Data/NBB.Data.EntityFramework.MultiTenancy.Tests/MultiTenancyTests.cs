using System;
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
        public void Should_add_tenantId()
        {
            var testTenantId = Guid.NewGuid();
            var tenantService = Mock.Of<ITenantService>(x =>
                x.GetCurrentTenantAsync() == Task.FromResult(new Tenant(testTenantId, "name")));
            var tenantDatabaseConfigService =
                Mock.Of<ITenantDatabaseConfigService>(x => x.IsSharedDatabase(testTenantId) && x.GetConnectionString(testTenantId) == "Test");
            var services = new ServiceCollection();
            services.AddSingleton(tenantService);
            services.AddSingleton(tenantDatabaseConfigService);

            services.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<TestDbContext>((sp, options) =>
                {
                    var tenantId = sp.GetRequiredService<ITenantService>().GetCurrentTenantAsync().Result.TenantId;
                    var conn = sp.GetRequiredService<ITenantDatabaseConfigService>().GetConnectionString(tenantId);
                    options.UseInMemoryDatabase(conn);
                });
            

            services.AddMultiTenantEfUow<TestEntity, TestDbContext>();

            var sp = services.BuildServiceProvider();

            var testEntity = new TestEntity {Id = 1};
            var dbContext = sp.GetRequiredService<TestDbContext>();
            dbContext.TestEntities.Add(testEntity);
            var uow = sp.GetRequiredService<IUow<TestEntity>>();

            //act
            uow.SaveChangesAsync();

            //assert
            dbContext.Entry(testEntity).GetTenantId().Should().Be(testTenantId);

        }
    }
}
