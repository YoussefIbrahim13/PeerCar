using Peer_Car.Domain.Common;
using Peer_Car.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.ConstrainedExecution;

namespace Peer_Car.Domain.Entities
{
    public class Booking : BaseEntity // BaseEntity بكون فيه الـ Id
    {
        // Foreign Keys
        public Guid CarId { get; set; }
        public Guid RenterId { get; set; }

        // Navigation Properties
        public virtual Car Car { get; set; } = null!;
        public virtual User Renter { get; set; } = null!;
        public virtual Payment? Payment { get; set; }

        // Booking Details
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }

        // Lifecycle Tracking
        public bool IsCarReceivedByUser { get; set; }
        public DateTime? CarReceivedDate { get; set; }

        public bool IsCarReturnedByUser { get; set; }
        public DateTime? CarReturnedDate { get; set; }

        // Financials
        public decimal? RefundAmount { get; set; }
        public bool IsRefunded { get; set; } = false;
        public bool IsKeyHandedOver { get; set; } = false;
    }
}
