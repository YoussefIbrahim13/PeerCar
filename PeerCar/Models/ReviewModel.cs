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

        [Required(ErrorMessage = "Please provide a comment")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        // Display properties
        public string ReviewerName { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public string FormattedDate => Date.ToString("MMM dd, yyyy");

        // Additional properties for car details
        public string? CarImagePath { get; set; }
        public string? CarOwnerName { get; set; }
        public string? CarOwnerEmail { get; set; }
        public string? CarBrand { get; set; }
        public string? CarModel { get; set; }
        public int? CarYear { get; set; }

        // Helper method to convert from domain model to view model
        public static ReviewModel FromReview(Review review)
        {
            var model = new ReviewModel
            {
                Id = review.Id,
                UserId = review.UserId,
                TargetId = review.TargetId,
                TargetType = review.TargetType,
                Comment = review.Comment,
                Date = review.Date,
                ReviewerName = review.User?.UserName ?? "Unknown User",
                CarImagePath = review.Car?.ImagePath,
                CarOwnerName = review.Car?.Owner?.Name,
                CarOwnerEmail = review.Car?.Owner?.Email,
                CarBrand = review.Car?.Brand,
                CarModel = review.Car?.Model,
                CarYear = review.Car?.Year
            };

            // Set target name based on type
            if (review.TargetType == Review.ReviewTargetType.Car && review.Car != null)
            {
                model.TargetName = $"#{review.Car.Id} - {review.Car.Brand} {review.Car.Model}";
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
                Comment = this.Comment,
                Date = this.Date
            };
        }
    }
}
