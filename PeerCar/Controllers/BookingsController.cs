using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CarRentalMVC.Controllers
{
    [Authorize]  // التأكد من أن المستخدم مسجل دخوله
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض كل الحجوزات للمستخدم
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.Identity.Name;  // الحصول على الـ UserName
            var bookings = await _context.Bookings
                .Where(b => b.Renter.Name == userId)  // استخدام اسم المستخدم لاستخراج الحجوزات الخاصة به
                .Include(b => b.Car)  // تضمين معلومات السيارة المرتبطة بالحجز
                .Include(b => b.Renter)  // تضمين معلومات المستأجر
                .ToListAsync();

            return View(bookings);
        }

        // عرض صفحة إنشاء حجز
        public IActionResult Create(int carId)
        {
            var car = _context.Cars.FirstOrDefault(c => c.Id == carId);
            if (car == null)
            {
                return NotFound();
            }

            // إعداد الحجز الجديد باستخدام تفاصيل السيارة
            var booking = new Booking
            {
                CarId = carId,
                TotalPrice = car.PricePerDay  // السعر يوميًا للسيارة
            };
            return View(booking);
        }

        // معالجة عملية إنشاء الحجز
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                // تعيين الـ RenterId إلى الـ User الحالي (المستخدم الذي قام بتسجيل الدخول)
                booking.RenterId = User.Identity.Name;

                // تحديث حالة الحجز إلى Pending (معلق) عند الإنشاء
                booking.Status = BookingStatus.Pending;

                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyBookings));  // إعادة توجيه إلى صفحة الحجوزات الخاصة بالمستخدم
            }

            return View(booking);
        }

        // عرض تفاصيل الحجز
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Renter)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // تحديث حالة الحجز إلى "مكتمل" أو "ملغي"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int bookingId, BookingStatus status)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            // تحديث حالة الحجز
            booking.Status = status;
            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyBookings));  // إعادة توجيه إلى صفحة الحجوزات الخاصة بالمستخدم
        }

        // إلغاء الحجز
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            // تحديث الحالة إلى "ملغى"
            booking.Status = BookingStatus.Cancelled;
            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyBookings));
        }
    }
}
