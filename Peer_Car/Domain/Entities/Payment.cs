using Peer_Car.Domain.Common;
using Peer_Car.Domain.Enums;

namespace Peer_Car.Domain.Entities
{
    public class Payment : BaseEntity
    {
     
        public Guid BookingId { get; set; }

        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Card"; 
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public DateTime? PaymentDate { get; set; }

        public string TransactionId { get; set; } = string.Empty;

        public virtual Booking? Booking { get; set; }
    }
}
