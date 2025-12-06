using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalMVC.Models
{
    public class CarSubmission
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string SubmittedById { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        public CarSubmissionStatus Status { get; set; } = CarSubmissionStatus.Pending;
        public string? AdminNotes { get; set; }
        public string? ApprovedById { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string? RejectionReason { get; set; }
        
        // Navigation properties
        public Car Car { get; set; } = null!;
        public User SubmittedBy { get; set; } = null!;
        public User? ApprovedBy { get; set; }
    }
} 