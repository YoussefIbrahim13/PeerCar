using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Peer_Car.Application.ViewModels
{
    public class ReviewViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Target ID is required")]
        public Guid TargetId { get; set; }

        [Required(ErrorMessage = "Booking ID is required")]
        public Guid BookingId { get; set; }

        public ReviewTargetType TargetType { get; set; }

        [Required(ErrorMessage = "Please provide a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please provide a comment")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string ReviewerName { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public string FormattedDate => CreatedAt.ToString("MMM dd, yyyy");

        public string? CarImagePath { get; set; }
        public string? CarOwnerName { get; set; }
        // 1. اضف الخاصية دي هنا عشان الـ View يشوفها
        public string? CarOwnerEmail { get; set; }
        public string? CarBrand { get; set; }
        public string? CarModel { get; set; }
        public int? CarYear { get; set; }

        public static ReviewViewModel FromReview(Review review)
        {
            if (review == null) return new ReviewViewModel();

            return new ReviewViewModel
            {
                Id = review.Id,
                UserId = review.UserId,
                TargetId = review.TargetId,
                BookingId = review.BookingId ?? Guid.Empty,
                Rating = review.Rating,
                TargetType = review.TargetType,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                ReviewerName = review.User?.FullName ?? "Unknown User",
                CarImagePath = review.Car?.ImagePath,
                CarBrand = review.Car?.Brand,
                CarModel = review.Car?.Model,
                CarYear = review.Car?.Year,
                // 2. اعمل Mapping للإيميل من الـ Owner الخاص بالعربية
                CarOwnerName = review.Car?.Owner?.FullName,
                CarOwnerEmail = review.Car?.Owner?.Email,
                TargetName = review.TargetType == ReviewTargetType.Car
                             ? $"{review.Car?.Brand} {review.Car?.Model}"
                             : review.User?.FullName ?? "User"
            };
        }
    }
}