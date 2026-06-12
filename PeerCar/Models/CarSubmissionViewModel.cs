using System.ComponentModel.DataAnnotations;

namespace CarRentalMVC.Models
{
    public class CarSubmissionViewModel
    {
        public int Id { get; set; }
        public Car Car { get; set; } = null!;
        public string SubmittedByName { get; set; } = string.Empty;
        public string SubmittedByEmail { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; }
        public CarSubmissionStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public string? AdminNotes { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovalDate { get; set; }
    }
} 