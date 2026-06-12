using Microsoft.EntityFrameworkCore;
using Peer_Car.Application.Interfaces;
using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;
using Peer_Car.Infrastructure.Data;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;

    public ReviewService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Read Operations

    public async Task<IEnumerable<ReviewViewModel>> GetAllReviewsAsync()
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Car)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(r => ReviewViewModel.FromReview(r));
    }

    public async Task<Review?> GetReviewByIdAsync(Guid id)
    {
        // إضافة Include لليوزر والعربية عشان صفحة الـ Details تعرض البيانات صح
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Car)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<(bool canReview, string message, Guid? carId, string? carName, string? carImagePath)> ValidateReviewEligibilityAsync(Guid bookingId, Guid userId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Car)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.RenterId == userId);

        // 1. لو الحجز مش موجود (بنرجع 5 قيم)
        if (booking == null)
            return (false, "Booking record not found.", null, null, null);

        // 2. لو الحجز لسه مخلصش (بنرجع 5 قيم)
        if (booking.Status != BookingStatus.Completed)
            return (false, "You can only review a trip after it's marked as Completed.", null, null, null);

        // 3. لو اليوزر قيم قبل كدة (بنرجع 5 قيم)
        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.BookingId == bookingId && r.UserId == userId);

        if (alreadyReviewed)
            return (false, "You have already submitted a review for this trip.", null, null, null);

        // 4. في حالة النجاح (بنرجع الـ 5 قيم كاملين بالترتيب)
        return (
            canReview: true,
            message: "Success",
            carId: booking.CarId,
            carName: $"{booking.Car.Brand} {booking.Car.Model}",
            carImagePath: booking.Car?.ImagePath // القيمة الخامسة اللي كانت ناقصة
        );
    }
    #endregion

    #region Write Operations

    public async Task CreateReviewAsync(Review review)
    {
        review.CreatedAt = DateTime.UtcNow; // تأكد إن الحقل ده موجود في الـ Entity
        _context.Reviews.Add(review); // إضافة التقييم للسياق (Context)
        await _context.SaveChangesAsync();
    }

    public async Task DeleteReviewAsync(Guid id)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review != null)
        {
            
            review.IsDeleted = true;

           
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Specialized Queries (Filters)

    public async Task<IEnumerable<Review>> GetReviewsByCarIdAsync(Guid carId)
    {
        return await _context.Reviews
            .Include(r => r.User) 
            .Include(r => r.Car) 
            .Where(r => r.TargetType == ReviewTargetType.Car && r.TargetId == carId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsByUserAsync(Guid userId)
    {
        return await _context.Reviews
            .Include(r => r.Car)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsAboutUserAsync(Guid targetUserId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.TargetType == ReviewTargetType.User && r.TargetId == targetUserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    #endregion
}