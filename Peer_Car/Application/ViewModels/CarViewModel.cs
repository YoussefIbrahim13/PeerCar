using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Peer_Car.Application.ViewModels
{
    public class CarViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Brand")]
        public string Brand { get; set; } = string.Empty;

        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        [Display(Name = "Price Per Day")]
        [DataType(DataType.Currency)]
        public decimal PricePerDay { get; set; }

        public string Location { get; set; } = string.Empty;

        [Display(Name = "Car Image")]
        public string? ImagePath { get; set; }

        public Guid OwnerId { get; set; }
        [Display(Name = "Owner")]
        public string OwnerName { get; set; } = string.Empty;

        // حالات السيارة
        public CarAvailabilityStatus AvailabilityStatus { get; set; }
        public CarSubmissionStatus SubmissionStatus { get; set; }

        // بيانات إحصائية (Calculated Fields)
        // هذه الحقول يتم حسابها في الـ Service قبل إرسال الموديل
        [Display(Name = "Rating")]
        public double AverageRating { get; set; }

        [Display(Name = "Reviews")]
        public int TotalReviews { get; set; }

        // خاصية مجمعة للعرض السريع
        public string FullName => $"{Brand} {Model} ({Year})";
    }
}
