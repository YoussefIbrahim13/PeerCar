using System;
using System.ComponentModel.DataAnnotations;

namespace CarRentalMVC.Models
{
    public class BookingViewModel
    {
        [Required]
        public int CarId { get; set; }

        [Required]
        public string RenterId { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Total price must be positive.")]
        public decimal TotalPrice { get; set; }

        public string? CarBrand { get; set; }
        public string? CarModel { get; set; }
        public decimal? PricePerDay { get; set; }
        public string? OwnerUserName { get; set; }
        // Added for stylish booking view
        public string? CarImagePath { get; set; }
        public int? CarYear { get; set; }
        public string? CarLocation { get; set; }
        public string? OwnerPhone { get; set; }
    }
}
