using Microsoft.EntityFrameworkCore;
using NBB.Payments.Domain.PayableAggregate;

namespace NBB.Payments.Data
{
    public class PaymentsDbContext : DbContext
    {
        public DbSet<Payable> Payables { get; set; }


        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payment>(builder =>
            {
                builder
                    .ToTable("Payments")
                    .HasKey(c => c.PaymentId);

                builder
                    .Property(c => c.PaymentId)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<Payable>(builder =>
            {
                builder
                    .ToTable("Payables")
                    .HasKey(c => c.PayableId);
                builder
                    .HasOne(payable => payable.Payment)
                    .WithOne()
                    .HasForeignKey<Payment>(payment => payment.PayableId);
                builder
                    .Property(c => c.PayableId)
                    .ValueGeneratedNever();
            });
        }
    }
}