using System.Collections.Generic;

namespace CarRentalMVC.Models
{
    public class UserProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public List<Car> Cars { get; set; } = new();
        public string Role { get; set; } = string.Empty;
        public double? AverageRating { get; set; }
        public string? Email { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
