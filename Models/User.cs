using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CarRentalMVC.Models
{
    public enum UserRole
    {
        Admin,
        User
    }

    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended,
        PendingVerification
    }

    public class User : IdentityUser
    {
        [Required, StringLength(100)]
        public required string Name { get; set; }

        [Required, StringLength(50)]
        public required string FirstName { get; set; }

        [Required, StringLength(50)]
        public required string LastName { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public UserStatus Status { get; set; } = UserStatus.Active;

        [DataType(DataType.Date)]
        public DateTime DateRegistered { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime? LastActive { get; set; }

        [Phone]
        [StringLength(20)]
        public new string? PhoneNumber { get; set; }

        [StringLength(256)]
        public string? PendingEmail { get; set; }

        [StringLength(256)]
        public string? ProfileImageUrl { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public ICollection<Car> OwnedCars { get; set; } = new List<Car>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
