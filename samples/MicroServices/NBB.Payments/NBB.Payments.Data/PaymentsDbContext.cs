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


            modelBuilder.Entity<Payable>()
                .ToTable("Payables")
                .HasKey(c => c.PayableId);

            modelBuilder.Entity<Payment>()
                .ToTable("Payments")
                .HasKey(c => c.PaymentId);

            modelBuilder.Entity<Payable>()
                .HasOne(payable => payable.Payment)
                .WithOne()
                .HasForeignKey<Payment>(payment => payment.PayableId);
        }
    }
}
