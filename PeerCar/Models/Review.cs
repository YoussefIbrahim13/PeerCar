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
        public int UserId { get; set; } // Reviewer

        [Required]
        public int TargetId { get; set; } // Car or Owner being reviewed

      
        public enum ReviewTargetType
        {
            Car,
            User
        }

        public ReviewTargetType TargetType { get; set; }


        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string Comment { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; }
        public int? CarId { get; set; }
        public Car Car { get; set; }
    }
}