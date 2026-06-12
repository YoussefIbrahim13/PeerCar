using Peer_Car.Application.ViewModels;

namespace Peer_Car.Application.Interfaces
{

    public interface IUserService
    {
        // الحصول على بيانات البروفايل كاملة وتحويلها لـ ViewModel
        Task<UserProfileViewModel?> GetUserProfileAsync(Guid userId);

        // ميثودز إضافية ممكن تحتاجها مستقبلاً
        Task<bool> UserExistsAsync(Guid userId);
    }
}
