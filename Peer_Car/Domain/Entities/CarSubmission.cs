using Peer_Car.Domain.Common;
using Peer_Car.Domain.Enums;

namespace Peer_Car.Domain.Entities
{
    public class CarSubmission : BaseEntity // بيرث Id (Guid), CreatedAt, IsDeleted
    {
        public Guid CarId { get; set; }
        public Guid SubmittedById { get; set; }
        public Guid? ApprovedById { get; set; } 

        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
        public CarSubmissionStatus Status { get; set; } = CarSubmissionStatus.Pending;

        public string? AdminNotes { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public virtual Car Car { get; set; } = null!;
        public virtual User SubmittedBy { get; set; } = null!;
        public virtual User? ApprovedBy { get; set; }
    }
}
