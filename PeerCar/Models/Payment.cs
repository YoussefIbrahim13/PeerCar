using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalMVC.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public required string PaymentMethod { get; set; } // Card, PayPal, Cash

        public enum PaymentStatus
        {
            Pending,
            Paid,
            Failed,
            Refunded
        }

        public PaymentStatus Status { get; set; }


        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public string TransactionId { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }
    }
}
