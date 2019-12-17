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
//    public class MultiTenancyEfAnnotationsTests
//    {
//        ITenantService _mockTenantService;
//        ITenantDatabaseConfigService _mockTenantDatabaseConfigService;
//        MultitenantDbContextHelper _multitenantDbContextHelper;

//        Guid tenant1Id = Guid.Parse("408e60ca-736e-404c-82a3-4a19e6e55afc");
//        Guid tenant2Id = Guid.Parse("ec460c8e-d3ca-4637-a0f1-739ce6427310");

//        public MultiTenancyEfAnnotationsTests()
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

//            var sut = new ContractMultitenantDbContext(_mockTenantService, _mockTenantDatabaseConfigService, _multitenantDbContextHelper);

//            sut.Database.EnsureDeleted();
//            sut.Database.EnsureCreated();


//            //Act
//            var contract = new Contract
//            {
//                Value = 100
//            };

//            sut.Contracts.Add(contract);
//            sut.SaveChanges();

//            //Assert
//            var tenantIdCompleted = Convert.ChangeType(sut.Entry(contract).Property("TenantId").CurrentValue, typeof(Guid));
//            tenantIdCompleted.Should().Be(tenant1Id);

//            //Cleanup
//            sut.Dispose();
//        }


//        [Fact]
//        public void Should_crossTenantDatabaseException_be_Thrown()
//        {
//            //Arrange
//            var sut = new ContractMultitenantDbContext(_mockTenantService, _mockTenantDatabaseConfigService, _multitenantDbContextHelper);

//            sut.Database.EnsureDeleted();
//            sut.Database.EnsureCreated();


//            //Act
//            // do work on tenant 1
//            var contract = new Contract { ContractId = 1, Value = 100 };
//            sut.Contracts.Add(contract);

//            var contract2 = new Contract { ContractId = 2, Value = 200 };
//            sut.Contracts.Add(contract2);

//            sut.SaveChanges();

//            sut.Entry(contract2).Property("TenantId").CurrentValue = tenant2Id;
//            sut.Entry(contract2).State = EntityState.Modified;
//            Exception ex = Assert.Throws<CrossTenantUpdateException>(() => sut.SaveChanges());

//            //Cleanup
//            sut.Dispose();
//        }

//    }

//    public class ContractMultitenantDbContext : MultiTenantDbContext
//    {

//        public static readonly LoggerFactory _myLoggerFactory =
//            new LoggerFactory(new[] { new DebugLoggerProvider() });

//        private readonly ITenantDatabaseConfigService _tenantDatabaseConfigService;
//        private Guid _tenantId;

//        public ContractMultitenantDbContext(ITenantService tenantService, ITenantDatabaseConfigService tenantDatabaseConfigService, MultitenantDbContextHelper multitenantDbContextHelper) : base(tenantService, multitenantDbContextHelper, tenantDatabaseConfigService)
//        {
//            _tenantDatabaseConfigService = tenantDatabaseConfigService;
//            _tenantId = tenantService.GetCurrentTenantAsync().GetAwaiter().GetResult().TenantId;
//        }

//        public DbSet<Contract> Contracts { get; set; }

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            optionsBuilder.UseInMemoryDatabase(_tenantDatabaseConfigService.GetConnectionString(_tenantId));
//            optionsBuilder.UseLoggerFactory(_myLoggerFactory);
//            optionsBuilder.EnableSensitiveDataLogging(true);
//            //base.OnConfiguring(optionsBuilder);
//        }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            modelBuilder.ApplyConfiguration<Contract>(new ContractConfiguration());
//            base.OnModelCreating(modelBuilder);            
//        }
//    }

//    class ContractConfiguration : MultiTenantEntityTypeConfiguration<Contract>
//    {

//    }

//    public class Contract
//    {
//        [Key]
//        public int ContractId { get; set; }
//        public double Value { get; set; }
//    }

//}