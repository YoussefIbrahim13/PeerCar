using Microsoft.AspNetCore.Identity;
using Peer_Car.Application.ViewModels;

namespace Peer_Car.Application.Interfaces
{
    public interface ICarSubmissionService
    {
        // الحصول على الطلبات المعلقة مجمعة حسب اسم المستخدم
        Task<IEnumerable<IGrouping<string, CarSubmissionViewModel>>> GetGroupedPendingSubmissionsAsync();

        // الحصول على تفاصيل طلب محدد
        Task<CarSubmissionViewModel?> GetSubmissionDetailsAsync(Guid id);

        // الموافقة على الطلب
        Task<IdentityResult> ApproveSubmissionAsync(Guid id, Guid adminId);

        // رفض الطلب مع ذكر السبب
        Task<IdentityResult> RejectSubmissionAsync(Guid id, string reason, Guid adminId);
    }
}
