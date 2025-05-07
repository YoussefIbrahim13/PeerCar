using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CarRentalMVC.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض صفحة الحجز الخاصة بالمستخدم
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.Identity.Name; // الحصول على معرف المستخدم من الجلسة الحالية
            var bookings = await _context.Bookings
                                          .Include(b => b.Car)
                                          .Where(b => b.RenterId ==userId)
                                          .ToListAsync();

            return View(bookings);
        }

        // عرض صفحة إنشاء الدفع
        public async Task<IActionResult> Create(int bookingId)
        {
            var booking = await _context.Bookings
                                         .Include(b => b.Car)
                                         .FirstOrDefaultAsync(b => b.Id == bookingId && b.RenterId == User.Identity.Name);

            if (booking == null)
            {
                return NotFound();
            }

            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = booking.Car.PricePerDay * (booking.EndDate - booking.StartDate).Days // حساب المبلغ بناءً على الأيام
            };

            return View(payment);
        }

        // معالجة الدفع
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                var booking = await _context.Bookings
                                             .FirstOrDefaultAsync(b => b.Id == payment.BookingId && b.RenterId == User.Identity.Name);

                if (booking == null)
                {
                    return NotFound();
                }

                booking.Status =BookingStatus.Pending; // تحديث حالة الدفع في الحجز
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyBookings)); // إعادة التوجيه إلى قائمة الحجوزات بعد الدفع
            }

            return View(payment);
        }
    }
}
