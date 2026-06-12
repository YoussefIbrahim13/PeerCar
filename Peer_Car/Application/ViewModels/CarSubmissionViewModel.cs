using Peer_Car.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Peer_Car.Application.ViewModels
{
    public class CarSubmissionViewModel
    {
        public Guid Id { get; set; }

        public Guid CarId { get; set; }
        public string CarBrand { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;

        [Display(Name = "Year")]
        public int CarYear { get; set; }

        [Display(Name = "Price Per Day")]
        public decimal CarPricePerDay { get; set; }

        [Display(Name = "Location")]
        public string CarLocation { get; set; } = string.Empty;

        public string? CarImagePath { get; set; }

        [Display(Name = "Legal Document")]
        public string? CarDocumentPath { get; set; }

        [Display(Name = "Car Details")]
        public string CarDescription { get; set; } = string.Empty;

        [Display(Name = "Submitted By")]
        public string SubmittedByName { get; set; } = string.Empty;

        [EmailAddress]
        public string SubmittedByEmail { get; set; } = string.Empty;

        [Display(Name = "Submission Date")]
        public DateTime SubmissionDate { get; set; }

        public CarSubmissionStatus Status { get; set; }

        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }

        [Display(Name = "Admin Notes")]
        public string? AdminNotes { get; set; }

        [Display(Name = "Approved By")]
        public string? ApprovedByName { get; set; }

        public DateTime? ApprovalDate { get; set; }
    }
}