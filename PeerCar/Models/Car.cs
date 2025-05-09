using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CarRentalMVC.Models
{
    public class Car
    {
        public int Id { get; set; }
        public required string Brand { get; set; }
        public required string Model { get; set; }
        public int Year { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerDay { get; set; }
        public required string Location { get; set; }

        [NotMapped]
        [Display(Name = "Car Image")]
        public IFormFile? ImageFile { get; set; }
        
        public required string ImageUrl { get; set; } = string.Empty;

        // Foreign key - updated from int to string for IdentityUser compatibility
        public required string OwnerId { get; set; } = string.Empty;

        // Navigation properties
        public required User Owner { get; set; }
        public enum CarAvailabilityStatus
        {
            Available,
            Rented,
            Maintenance,
            Reserved
        }

        
        public required ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public CarAvailabilityStatus AvailabilityStatus { get; set; }
    }
}
