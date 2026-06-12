using Peer_Car.Domain.Entities;

namespace Peer_Car.Application.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCars { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int PendingSubmissions { get; set; }
        public List<Booking> RecentBookings { get; set; } = new();
    }
}
