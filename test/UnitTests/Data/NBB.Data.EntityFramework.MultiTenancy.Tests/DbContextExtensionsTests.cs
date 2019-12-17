using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;

namespace NBB.Data.EntityFramework.MultiTenancy.Tests
{
    class DbContextExtensionsTests
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<Pending>")]
        public void Should_()
        {
            var modelBuilder = new ModelBuilder(Mock.Of<IMutableModel>());

            modelBuilder.ApplyMultiTenantConfiguration(Mock.Of<IEntityTypeConfiguration<TestEntity>>(), Mock.Of<IServiceProvider>());

            //cannot assert 

        }
    }


    public class TestDbContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var sp = this.GetInfrastructure();
            modelBuilder.ApplyMultiTenantConfiguration(Mock.Of<IEntityTypeConfiguration<TestEntity>>(), sp);

        }
    }

    public class TestEntity
    {

    }
}
