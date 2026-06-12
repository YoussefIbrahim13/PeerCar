using Microsoft.AspNetCore.Identity;
using Peer_Car.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Peer_Car.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        public UserRole Role { get; set; } = UserRole.Renter;
        public UserStatus Status { get; set; } = UserStatus.Active;

        public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
        public DateTime? LastActive { get; set; }

        public string? ProfileImageUrl { get; set; }

        public bool IsDeleted { get; set; } = false;

        [StringLength(256)]
        public string? PendingEmail { get; set; }
        public virtual ICollection<Car> OwnedCars { get; set; } = new List<Car>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();



        public string? NationalIdFrontUrl { get; set; }
        public string? NationalIdBackUrl { get; set; }
        public string? LicenseFrontUrl { get; set; }
        public string? LicenseBackUrl { get; set; }

        public bool IsDocumentsVerified { get; set; } = false;

        public DocumentStatus DocumentStatus { get; set; } = DocumentStatus.NotUploaded;
    }
}
