using System;
using System.ComponentModel.DataAnnotations;
using CarRentalMVC.Models;

namespace PeerCar.Models
{
    public class ReviewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Target ID is required")]
        public int TargetId { get; set; }

        public Review.ReviewTargetType TargetType { get; set; }

        [Required(ErrorMessage = "Please provide a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please provide a comment")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        // Display properties
        public string ReviewerName { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public string FormattedDate => Date.ToString("MMM dd, yyyy");
        
        // Helper method to convert from domain model to view model
        public static ReviewModel FromReview(Review review)
        {
            var model = new ReviewModel
            {
                Id = review.Id,
                UserId = review.UserId,
                TargetId = review.TargetId,
                TargetType = review.TargetType,
                Rating = review.Rating,
                Comment = review.Comment,
                Date = review.Date,
                ReviewerName = review.User?.UserName ?? "Unknown User"
            };

            // Set target name based on type
            if (review.TargetType == Review.ReviewTargetType.Car && review.Car != null)
            {
                model.TargetName = $"{review.Car.Brand} {review.Car.Model}";
            }
            else if (review.TargetType == Review.ReviewTargetType.User && review.User != null)
            {
                model.TargetName = review.User.UserName ?? $"User #{review.TargetId}";
            }
            else
            {
                model.TargetName = $"{review.TargetType} #{review.TargetId}";
            }

            return model;
        }

        // Helper method to convert from view model to domain model
        public Review ToReview()
        {
            return new Review
            {
                Id = this.Id,
                UserId = this.UserId,
                TargetId = this.TargetId,
                TargetType = this.TargetType,
                Rating = this.Rating,
                Comment = this.Comment,
                Date = this.Date
            };
        }
    }
}
