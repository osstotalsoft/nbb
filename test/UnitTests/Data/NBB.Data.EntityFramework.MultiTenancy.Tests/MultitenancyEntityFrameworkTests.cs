//using FluentAssertions;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.Debug;
//using Moq;
//using NBB.MultiTenancy.Abstractions.Services;
//using NBB.MultiTenancy.Data.Abstractions;
//using System;
//using System.ComponentModel.DataAnnotations;
//using System.Threading.Tasks;
//using Xunit;

//namespace NBB.Data.EntityFramework.MultiTenancy.Tests
//{
//    public class MultitenancyEntityFrameworkTests
//    {
//        ITenantService _mockTenantService;
//        ITenantDatabaseConfigService _mockTenantDatabaseConfigService;
//        MultitenantDbContextHelper _multitenantDbContextHelper;

//        Guid tenant1Id = Guid.Parse("408e60ca-736e-404c-82a3-4a19e6e55afc");
//        Guid tenant2Id = Guid.Parse("ec460c8e-d3ca-4637-a0f1-739ce6427310");

//        public MultitenancyEntityFrameworkTests()
//        {
//            var mockDatabaseConfigService = new Mock<ITenantDatabaseConfigService>();
//            mockDatabaseConfigService.Setup(x => x.GetConnectionString(tenant1Id)).Returns("SharedDatabase");
//            mockDatabaseConfigService.Setup(x => x.IsSharedDatabase(tenant1Id)).Returns(true);

//            mockDatabaseConfigService.Setup(x => x.GetConnectionString(tenant2Id)).Returns("SharedDatabase");
//            mockDatabaseConfigService.Setup(x => x.IsSharedDatabase(tenant2Id)).Returns(true);

//            _mockTenantDatabaseConfigService = mockDatabaseConfigService.Object;

//            var mockTenantService = new Mock<ITenantService>();
//            var tenant1 = new NBB.MultiTenancy.Abstractions.Tenant
//            {
//                TenantId = tenant1Id,
//                Name = "tenant1"
//            };

//            mockTenantService.Setup(x => x.GetCurrentTenantAsync()).Returns(Task.FromResult(tenant1));

//            _mockTenantService = mockTenantService.Object;

//            _multitenantDbContextHelper = new MultitenantDbContextHelper(mockDatabaseConfigService.Object, _mockTenantService);

//        }


//        [Fact]
//        public void Should_tenantId_Be_completed()
//        {
//            //Arrange

//            var sut = new MockMultitenantDbContext(_mockTenantService, _mockTenantDatabaseConfigService, _multitenantDbContextHelper);

//            sut.Database.EnsureDeleted();
//            sut.Database.EnsureCreated();


//            //Act
//            var invoice = new Invoice
//            {
//                Value = 100
//            };

//            sut.Invoices.Add(invoice);
//            sut.SaveChanges();

//            //Assert
//            var tenantIdCompleted = Convert.ChangeType(sut.Entry(invoice).Property("TenantId").CurrentValue, typeof(Guid));
//            tenantIdCompleted.Should().Be(tenant1Id);

//            //Cleanup
//            sut.Dispose();
//        }


//        [Fact]
//        public void Should_crossTenantDatabaseException_be_Thrown()
//        {
//            //Arrange
//            var sut = new MockMultitenantDbContext(_mockTenantService, _mockTenantDatabaseConfigService, _multitenantDbContextHelper);

//            sut.Database.EnsureDeleted();
//            sut.Database.EnsureCreated();


//            //Act
//            // do work on tenant 1
//            var invoice = new Invoice { InvoiceId = 1, Value = 100 };
//            sut.Invoices.Add(invoice);

//            var invoice2 = new Invoice { InvoiceId = 2, Value = 200 };
//            sut.Invoices.Add(invoice2);

//            sut.SaveChanges();

//            sut.Entry(invoice2).Property("TenantId").CurrentValue = tenant2Id;
//            sut.Entry(invoice2).State = EntityState.Modified;
//            Exception ex = Assert.Throws<CrossTenantUpdateException>(() => sut.SaveChanges());

//            //Cleanup
//            sut.Dispose();
//        }

//    }

//    public class MockMultitenantDbContext : MultiTenantDbContext
//    {

//        public static readonly LoggerFactory _myLoggerFactory =
//            new LoggerFactory(new[] { new DebugLoggerProvider() });

//        private readonly ITenantDatabaseConfigService _tenantDatabaseConfigService;
//        private Guid _tenantId;

//        public MockMultitenantDbContext(ITenantService tenantService, ITenantDatabaseConfigService tenantDatabaseConfigService, MultitenantDbContextHelper multitenantDbContextHelper) : base(tenantService, multitenantDbContextHelper, tenantDatabaseConfigService)
//        {
//            _tenantDatabaseConfigService = tenantDatabaseConfigService;
//            _tenantId = tenantService.GetCurrentTenantAsync().GetAwaiter().GetResult().TenantId;
//        }

//        public DbSet<Invoice> Invoices { get; set; }

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            optionsBuilder.UseInMemoryDatabase(_tenantDatabaseConfigService.GetConnectionString(_tenantId));
//            optionsBuilder.UseLoggerFactory(_myLoggerFactory);
//            optionsBuilder.EnableSensitiveDataLogging(true);
//            //base.OnConfiguring(optionsBuilder);
//        }
//    }

//    [MustHaveTenant]
//    public class Invoice
//    {
//        [Key]
//        public int InvoiceId { get; set; }
//        public double Value { get; set; }
//    }
//}