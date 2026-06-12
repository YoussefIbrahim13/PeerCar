using Microsoft.EntityFrameworkCore;
using Peer_Car.Application.Interfaces;
using Peer_Car.Application.ViewModels;
using Peer_Car.Infrastructure.Data;

namespace Peer_Car.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileViewModel?> GetUserProfileAsync(Guid userId)
        {
            // جلب المستخدم مع سياراته (فقط السيارات المقبولة Approved)
            var user = await _context.Users
                .Include(u => u.OwnedCars.Where(c => c.SubmissionStatus == Domain.Enums.CarSubmissionStatus.Approved))
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            // Mapping يدوي من الـ Entity للـ ViewModel
            return new UserProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName ?? user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                EmailConfirmed = user.EmailConfirmed,

                // التعامل مع مسار الصورة
                ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImageUrl)
                    ? "/images/default-profile.png"
                    : user.ProfileImageUrl,

                // تحويل لستة السيارات لـ ViewModels لو الـ ViewModel بتاعك بيطلب كدة
                Cars = user.OwnedCars.Select(c => new CarViewModel
                {
                    Id = c.Id,
                    Brand = c.Brand,
                    Model = c.Model,
                    Year = c.Year,
                    PricePerDay = c.PricePerDay,
                    ImagePath = c.ImagePath,
                    Location = c.Location,
                    AvailabilityStatus = c.AvailabilityStatus
                }).ToList(),
                AverageRating = null // كما طلبت في الكود الأصلي
            };
        }

        public async Task<bool> UserExistsAsync(Guid userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }
    }
}
