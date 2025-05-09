using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /admin
        [Route("")]
        [Route("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            // Get summary counts for the dashboard
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalCars = await _context.Cars.CountAsync();
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();
            ViewBag.PendingBookings = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Pending)
                .CountAsync();
            
            // Get recent bookings for quick overview
            var recentBookings = await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Renter)
                .OrderByDescending(b => b.Id)
                .Take(5)
                .ToListAsync();
                
            return View(recentBookings);
        }

        // GET: /admin/bookings
        [Route("bookings")]
        public async Task<IActionResult> Bookings()
        {
            var bookings = await _context.Bookings
                                          .Include(b => b.Car)
                                          .Include(b => b.Renter)
                                          .ToListAsync();
            return View(bookings);
        }

        // GET: /admin/users
        [Route("users")]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // GET: /admin/cars
        [Route("cars")]
        public async Task<IActionResult> Cars()
        {
            var cars = await _context.Cars.Include(c => c.Owner).ToListAsync();
            return View(cars);
        }

        // GET: /admin/bookings/{id}
        [Route("bookings/{id}")]
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

        // POST: /admin/bookings/update-status
        [HttpPost]
        [Route("bookings/update-status")]
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

        // POST: /admin/users/update-status
        [HttpPost]
        [Route("users/update-status")]
        public async Task<IActionResult> UpdateUserStatus(string userId, string status)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // تحويل النص إلى حالة المستخدم
            if (Enum.TryParse(status, out UserStatus userStatus))
            {
                user.Status = userStatus;
                user.LastActive = DateTime.Now;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Users));
            }

            return BadRequest("Invalid status.");
        }

        // POST: /admin/bookings/delete
        [HttpPost]
        [Route("bookings/delete")]
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

        // POST: /admin/users/delete
        [HttpPost]
        [Route("users/delete")]
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

        // POST: /admin/cars/delete
        [HttpPost]
        [Route("cars/delete")]
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
