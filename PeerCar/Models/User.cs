using System.ComponentModel.DataAnnotations;

namespace CarRentalMVC.Models
{
    public enum UserRole
    {
        Owner,
        Renter
    }

    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } // استخدم تجزئة بدلاً من كلمة المرور نفسها

        [Phone]
        public string Phone { get; set; }

        [Required]
        public UserRole Role { get; set; }

        // Navigation properties
        public ICollection<Car> OwnedCars { get; set; } = new List<Car>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
