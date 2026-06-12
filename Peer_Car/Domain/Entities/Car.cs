using Microsoft.AspNetCore.Mvc.ViewEngines;
using Peer_Car.Domain.Common;
using Peer_Car.Domain.Enums;

namespace Peer_Car.Domain.Entities
{
    public class Car : BaseEntity // بيرث الـ Id (Guid) والـ CreatedAt أوتوماتيكياً
    {
        public required string Brand { get; set; }
        public required string Model { get; set; }
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }
        public required string Location { get; set; }
      
        // Foreign key - تم تغييره لـ Guid
        public Guid OwnerId { get; set; }
        public virtual User? Owner { get; set; }

        // Enums (تم نقل تعريفهم لملفات مستقلة)
        public CarAvailabilityStatus AvailabilityStatus { get; set; }
        public CarSubmissionStatus SubmissionStatus { get; set; } = CarSubmissionStatus.Pending;

        // Media & Docs
        public string? ImagePath { get; set; }
        public string? DocumentPath { get; set; }

        // Approval System
        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow; // تاريخ التقديم

        public DateTime? ApprovalDate { get; set; }
        public string? RejectionReason { get; set; }

        // Collections
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public bool IsDeleted { get; set; } = false;
    }
}
