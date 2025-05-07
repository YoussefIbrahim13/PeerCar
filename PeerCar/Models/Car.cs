using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalMVC.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerDay { get; set; }
        public string Location { get; set; }

        [NotMapped]
        [Display(Name = "صورة السيارة")]
        public IFormFile ImageFile { get; set; }
        
        public string ImageUrl { get; set; }

        // Foreign key
        public int OwnerId { get; set; }

        // Navigation properties
        public User Owner { get; set; }
        public enum CarAvailabilityStatus
    {
        Available,
        Rented,
        Maintenance,
        Reserved
    }

        
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public CarAvailabilityStatus AvailabilityStatus { get; set; }
    }
}