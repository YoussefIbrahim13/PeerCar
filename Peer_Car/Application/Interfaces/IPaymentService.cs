using Microsoft.AspNetCore.Identity;
using Peer_Car.Domain.Entities;

namespace Peer_Car.Application.Interfaces
{
    public interface IPaymentService
    {
        // عرض الحجوزات التي تحتاج دفع
        Task<IEnumerable<Booking>> GetUserBookingsForPaymentAsync(Guid userId);

        // تجهيز بيانات الدفع (حساب المبلغ بناءً على الأيام)
        Task<Payment?> PreparePaymentAsync(Guid bookingId, Guid userId);

        // تنفيذ عملية الدفع وتأكيد الحجز
        Task<IdentityResult> ProcessPaymentAsync(Payment payment, Guid userId);
    }
}
