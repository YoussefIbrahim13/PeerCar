using Peer_Car.Domain.Common;
using Peer_Car.Domain.Enums;

namespace Peer_Car.Domain.Entities
{
    public class Review : BaseEntity 
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public Guid TargetId { get; set; }
        public ReviewTargetType TargetType { get; set; }

        public Guid? BookingId { get; set; }
        public virtual Booking? Booking { get; set; }

        public Guid? CarId { get; set; }
        public virtual Car? Car { get; set; }

        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

  
        public string? AuthorName { get; set; }
    }
}
