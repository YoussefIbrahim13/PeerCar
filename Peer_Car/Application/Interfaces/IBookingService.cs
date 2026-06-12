using Microsoft.AspNetCore.Identity;
using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;

namespace Peer_Car.Application.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid userId);
        Task<Booking?> GetBookingDetailsAsync(Guid id);
        Task<BookingViewModel?> PrepareBookingViewModelAsync(Guid carId, Guid userId);
        Task<bool> IsCarAvailableAsync(Guid carId, DateTime start, DateTime end);
        Task<IdentityResult> CreateBookingAsync(BookingViewModel model, string paymentMethod);
        Task<bool> CancelBookingAsync(Guid id, Guid userId);
        Task UpdateStatusAsync(Guid id, BookingStatus status);
        Task<bool> ConfirmPickupAsync(Guid id, Guid ownerId);  // للمالك فقط
        Task<bool> ConfirmReturnAsync(Guid id, Guid renterId); // للمستأجر فقط
        Task DeleteBookingAsync(Guid id);
        Task<bool> ApproveBookingAsync(Guid id, Guid ownerId);
        Task<IEnumerable<Car>> GetOwnerCarsWithBookingsAsync(Guid ownerId);

        Task<bool> RequestReturnAsync(Guid id, Guid renterId);

        Task<bool> FinalizeBookingAsync(Guid id, Guid ownerId);

    }
}
