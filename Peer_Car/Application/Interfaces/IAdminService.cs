using Microsoft.AspNetCore.Identity;
using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;

namespace Peer_Car.Application.Interfaces
{
    public interface IAdminService
    {
        // Dashboard & Stats
        Task<AdminDashboardViewModel> GetDashboardDataAsync();

        // User Management
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IdentityResult> CreateUserAsync(CreateUserViewModel model);
        Task<IdentityResult> ToggleAdminRoleAsync(Guid userId);
        Task UpdateUserStatusAsync(Guid userId, UserStatus status);
        Task SoftDeleteUserAsync(Guid userId);
        Task UpdateUserRoleAsync(Guid userId, UserRole role);
        Task<bool> DeleteUserByEmailAsync(string email);

        // Booking Management
        Task<IEnumerable<Booking>> GetBookingsAsync(BookingStatus? status);
        Task<Booking?> GetBookingDetailsAsync(Guid id);
        Task UpdateBookingStatusAsync(Guid id, BookingStatus status);
        Task DeleteBookingAsync(Guid id);
        Task<bool> ConfirmRefundAsync(Guid id);

        // Car Management
        Task<IEnumerable<Car>> GetAllCarsAsync();
        Task DeleteCarAsync(Guid carId);

        // Specialized Queries
        Task<IEnumerable<User>> GetUsersWithReservationsAsync();
        Task<(IEnumerable<User> Users, int TotalCount)> GetDeletedUsersAsync(int page, int pageSize, string search);

        Task<User?> GetUserDetailsAsync(Guid id);
        Task<IEnumerable<User>> GetUsersPendingVerificationAsync();
        Task<bool> ProcessUserVerificationAsync(Guid userId, bool approve);
        Task<IEnumerable<User>> GetDeletedUsersAsync();
        Task<IEnumerable<Car>> GetDeletedCarsAsync();
        Task<IEnumerable<Booking>> GetDeletedBookingsAsync();
        

    }
}
