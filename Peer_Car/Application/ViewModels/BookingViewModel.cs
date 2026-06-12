using System.ComponentModel.DataAnnotations;

namespace Peer_Car.Application.ViewModels
{
    public class BookingViewModel
    {
        [Required]
        public Guid CarId { get; set; }

        [Required]
        public Guid RenterId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be positive.")]
        public decimal TotalPrice { get; set; }


        public string? CarBrand { get; set; }
        public string? CarModel { get; set; }
        public decimal? PricePerDay { get; set; }
        public string? OwnerUserName { get; set; }
        public string? CarImagePath { get; set; }
        public int? CarYear { get; set; }
        public string? CarLocation { get; set; }
        public string? OwnerPhone { get; set; }


        public IFormFile? IdFrontFile { get; set; }
        public IFormFile? IdBackFile { get; set; }
        public IFormFile? LicenseFrontFile { get; set; }
        public IFormFile? LicenseBackFile { get; set; }

        public bool RequiresDocuments { get; set; }
    }
}
