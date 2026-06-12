using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;

namespace Peer_Car.Application.Interfaces
{
    public interface ICarService
    {
        // Public & User Actions
        Task<IEnumerable<Car>> GetApprovedCarsAsync(string? brand, string? priceRange);
        Task<Car?> GetCarByIdAsync(Guid id);
        Task<IEnumerable<string>> GetDistinctBrandsAsync();
        Task<UserCarViewModel> GetUserCarsAndSubmissionsAsync(Guid userId);

        // Admin Actions
        //Task CreateCarByAdminAsync(Car car, IFormFile? image, Guid adminId);
        Task UpdateCarAsync(Guid id, Car car, IFormFile? image);
        Task DeleteCarAsync(Guid id);

        // User Submission
        Task SubmitCarByUserAsync(Car car, IFormFile image, IFormFile document, Guid userId);
        Task ResubmitRejectedCarAsync(Guid carId, Car updatedCar, IFormFile? image, IFormFile? document, Guid userId);

        Task<IEnumerable<CarViewModel>> GetRecentCarsAsync(int count);
    }
}
