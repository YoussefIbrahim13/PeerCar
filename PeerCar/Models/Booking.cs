using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CarRentalMVC.Models
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required]
        public required string RenterId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public BookingStatus Status { get; set; }

        [ForeignKey("CarId")]
        public required Car Car { get; set; }

        [ForeignKey("RenterId")]
        public required User Renter { get; set; }

        public Payment? Payment { get; set; }

        public bool IsCarReceivedByUser { get; set; }
        public DateTime? CarReceivedDate { get; set; }
        public bool IsCarReturnedByUser { get; set; }
        public DateTime? CarReturnedDate { get; set; }
        public decimal? RefundAmount { get; set; }
        public bool IsRefunded { get; set; } = false;
    }
}
