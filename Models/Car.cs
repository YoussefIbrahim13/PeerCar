using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CarRentalMVC.Models
{
    public enum CarSubmissionStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class Car
    {
        public int Id { get; set; }
        public required string Brand { get; set; }
        public required string Model { get; set; }
        public int Year { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerDay { get; set; }
        public required string Location { get; set; }

        // Foreign key - updated from int to string for IdentityUser compatibility
        public required string OwnerId { get; set; } = string.Empty;

        // Navigation properties
        public User? Owner { get; set; }
        public enum CarAvailabilityStatus
        {
            Available,
            Rented,
            Maintenance,
            Reserved
        }

        
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public CarAvailabilityStatus AvailabilityStatus { get; set; }
        [StringLength(255)]
        public string? ImagePath { get; set; }

        // New properties for submission and approval system
        public CarSubmissionStatus SubmissionStatus { get; set; } = CarSubmissionStatus.Pending;
        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        public DateTime? ApprovalDate { get; set; }
        public string? RejectionReason { get; set; }
        [StringLength(255)]
        public string? DocumentPath { get; set; }
    }
}
