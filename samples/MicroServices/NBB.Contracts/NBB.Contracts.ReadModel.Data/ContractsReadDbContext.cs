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

            modelBuilder.Entity<ContractReadModel>(builder =>
            {
                builder
                    .ToTable("Contracts")
                    .HasKey(c => c.ContractId);

                builder.HasMany(c => c.ContractLines)
                    .WithOne()
                    .HasForeignKey(cl => cl.ContractId);

                builder
                    .Property(x => x.ContractId)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<ContractLineReadModel>(builder =>
            {
                builder
                    .ToTable("ContractLines")
                    .HasKey(c => c.ContractLineId);

                builder
                    .Property(x => x.ContractLineId)
                    .ValueGeneratedNever();
            });
        }
    }
}