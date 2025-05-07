using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalMVC.Controllers
{
    [Authorize(Roles = "Admin")] // التأكد أن المستخدم لديه صلاحيات الادمن
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض قائمة بالحجوزات
        public async Task<IActionResult> Bookings()
        {
            var bookings = await _context.Bookings
                                          .Include(b => b.Car)
                                          .Include(b => b.Renter)
                                          .ToListAsync();
            return View(bookings);
        }

        // عرض قائمة بالمستخدمين
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // عرض قائمة بالسيارات
        public async Task<IActionResult> Cars()
        {
            var cars = await _context.Cars.Include(c => c.Owner).ToListAsync();
            return View(cars);
        }

        // عرض تفاصيل حجز
        public async Task<IActionResult> BookingDetails(int id)
        {
            var booking = await _context.Bookings
                                         .Include(b => b.Car)
                                         .Include(b => b.Renter)
                                         .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // تغيير حالة الحجز
        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int id, string status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // تحويل النص إلى حالة الحجز
            if (Enum.TryParse(status, out BookingStatus bookingStatus))
            {
                booking.Status = bookingStatus;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Bookings));
            }

            return BadRequest("Invalid status.");
        }

        // حذف حجز
        [HttpPost]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Bookings));
        }

        // حذف مستخدم
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Users));
        }

        // حذف سيارة
        [HttpPost]
        public async Task<IActionResult> DeleteCar(int carId)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car == null)
            {
                return NotFound();
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Cars));
        }
    }
}
