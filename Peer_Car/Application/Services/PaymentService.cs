using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peer_Car.Application.Interfaces;
using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;
using Peer_Car.Infrastructure.Data;

namespace Peer_Car.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsForPaymentAsync(Guid userId)
        {
            return await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.RenterId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Payment?> PreparePaymentAsync(Guid bookingId, Guid userId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.RenterId == userId);

            if (booking == null || booking.Car == null) return null;

            // حساب عدد الأيام (بحد أدنى يوم واحد)
            var days = (booking.EndDate - booking.StartDate).Days;
            if (days <= 0) days = 1;

            return new Payment
            {
                BookingId = booking.Id,
                Amount = booking.Car.PricePerDay * days,
                Status = PaymentStatus.Pending,
                PaymentMethod = "Credit Card",
                TransactionId = Guid.NewGuid().ToString() // توليد رقم معاملة تجريبي
            };
        }

        public async Task<IdentityResult> ProcessPaymentAsync(Payment payment, Guid userId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == payment.BookingId && b.RenterId == userId);

            if (booking == null)
                return IdentityResult.Failed(new IdentityError { Description = "Booking not found." });

            // 1. تحديث حالة الحجز لـ Confirmed
            booking.Status = BookingStatus.Confirmed;

            // 2. تحديث حالة الدفع لـ Succeeded (بافتراض نجاح العملية)
            payment.Status = PaymentStatus.Succeeded;
            payment.CreatedAt = DateTime.UtcNow;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }
    }
}
