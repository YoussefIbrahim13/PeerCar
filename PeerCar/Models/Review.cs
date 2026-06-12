using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalMVC.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; } // Changed from int to string for IdentityUser compatibility

        [Required]
        public int TargetId { get; set; } // Car or Owner being reviewed

        [Required]
        public int? BookingId { get; set; } // Booking being reviewed

        public enum ReviewTargetType
        {
            Car,
            User
        }

        public ReviewTargetType TargetType { get; set; }


        [Required]
        [StringLength(500)]
        public string Comment { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public int? CarId { get; set; }
        public Car? Car { get; set; }
        public string? AuthorName { get; set; }
    }
}
