using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Peer_Car.Domain.Entities;

namespace Peer_Car.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<CarSubmission> CarSubmissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region 1. Global Decimal Precision
            var decimalProperties = builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

            foreach (var property in decimalProperties)
            {
                property.SetColumnType("decimal(18,2)");
            }
            #endregion

            #region 2. Global Query Filters (Soft Delete Logic)
            builder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<Car>().HasQueryFilter(c => !c.IsDeleted);

            builder.Entity<Booking>().HasQueryFilter(b => !b.Car.IsDeleted && !b.Renter.IsDeleted);

            builder.Entity<Review>().HasQueryFilter(r => !r.User.IsDeleted);
            builder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted);
            #endregion

            #region 3. Relationships Configuration & Warning Fixes

            builder.Entity<Car>()
                .HasOne(c => c.Owner)
                .WithMany(u => u.OwnedCars)
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.Entity<Booking>()
                .HasOne(b => b.Renter)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.RenterId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.Entity<Booking>()
                .HasOne(b => b.Car)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CarId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .IsRequired(false);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.Entity<CarSubmission>()
                .HasOne(cs => cs.SubmittedBy)
                .WithMany()
                .HasForeignKey(cs => cs.SubmittedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.Entity<CarSubmission>()
                .HasOne(cs => cs.ApprovedBy)
                .WithMany()
                .HasForeignKey(cs => cs.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

          

            builder.Entity<CarSubmission>()
                .HasOne(cs => cs.Car) 
                .WithMany()           
                .HasForeignKey(cs => cs.CarId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false); 

            #endregion
        }
    }
}