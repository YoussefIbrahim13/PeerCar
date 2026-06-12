namespace Peer_Car.Application.ViewModels
{
    public class UserProfileViewModel
    {
       public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string? PhoneNumber { get; set; }

        public List<CarViewModel> Cars { get; set; } = new List<CarViewModel>();
        public string Role { get; set; } = string.Empty;
        public double? AverageRating { get; set; }
        public string? Email { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
