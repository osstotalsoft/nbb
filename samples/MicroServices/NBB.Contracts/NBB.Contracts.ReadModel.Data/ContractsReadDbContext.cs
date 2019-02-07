using Microsoft.EntityFrameworkCore;

namespace NBB.Contracts.ReadModel.Data
{
    public class ContractsReadDbContext : DbContext
    {
        public DbSet<ContractReadModel> Contracts { get; set; }

        public ContractsReadDbContext(DbContextOptions<ContractsReadDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<ContractReadModel>()
                .ToTable("Contracts")
                .HasKey(c => c.ContractId);

            modelBuilder.Entity<ContractReadModel>()
                .HasMany(c => c.ContractLines)
                .WithOne()
                .HasForeignKey(cl => cl.ContractId);

            modelBuilder.Entity<ContractLineReadModel>()
                .ToTable("ContractLines")
                .HasKey(c => c.ContractLineId);

        }
    }
}
