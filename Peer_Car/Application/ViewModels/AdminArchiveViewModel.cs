using Peer_Car.Domain.Entities;

namespace Peer_Car.Application.ViewModels
{
    public class AdminArchiveViewModel
    {
        public IEnumerable<User> DeletedUsers { get; set; } = new List<User>();
        public IEnumerable<Car> DeletedCars { get; set; } = new List<Car>();
        public IEnumerable<Booking> DeletedBookings { get; set; } = new List<Booking>();
    }
}
