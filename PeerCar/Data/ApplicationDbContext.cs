using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CarRentalMVC.Models;

namespace CarRentalMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // جداول قاعدة البيانات
        public DbSet<Car> Cars { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<CarSubmission> CarSubmissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Cascade delete Cars when User is deleted
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Owner)
                .WithMany(u => u.OwnedCars)
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cascade delete Bookings when User is deleted (as Renter)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Renter)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.RenterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cascade delete Reviews when User is deleted
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cascade delete Bookings when Car is deleted
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Car)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cascade delete Reviews when Car is deleted
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Car)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cascade delete Payments when Booking is deleted
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // CarSubmission relationships
            modelBuilder.Entity<CarSubmission>()
                .HasOne(cs => cs.Car)
                .WithMany()
                .HasForeignKey(cs => cs.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CarSubmission>()
                .HasOne(cs => cs.SubmittedBy)
                .WithMany()
                .HasForeignKey(cs => cs.SubmittedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarSubmission>()
                .HasOne(cs => cs.ApprovedBy)
                .WithMany()
                .HasForeignKey(cs => cs.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Car>()
              .Property(c => c.PricePerDay)
              .HasColumnType("decimal(18,2)");

            // Note: Reviews where TargetType = User and TargetId = user.Id are not a direct FK, so must be deleted manually if needed.
        }
    }
}
