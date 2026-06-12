using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;

namespace Peer_Car.Application.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewViewModel>> GetAllReviewsAsync();
        Task<Review?> GetReviewByIdAsync(Guid id);
        Task<(bool canReview, string message, Guid? carId, string? carName, string? carImagePath)> ValidateReviewEligibilityAsync(Guid bookingId, Guid userId);
        Task CreateReviewAsync(Review review);
        Task DeleteReviewAsync(Guid id);

        // فلاتر التقييمات
        Task<IEnumerable<Review>> GetReviewsByCarIdAsync(Guid carId);
        Task<IEnumerable<Review>> GetReviewsByUserAsync(Guid userId); // التقييمات اللي كتبها اليوزر
        Task<IEnumerable<Review>> GetReviewsAboutUserAsync(Guid targetUserId); // التقييمات اللي اتكتبت عن اليوزر
    }
}
