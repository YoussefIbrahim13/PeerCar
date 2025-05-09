using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CarRentalMVC.Controllers
{
    [Authorize]  // Ensures the user is logged in
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all bookings for the user
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.Identity?.Name;  // Get UserName considering potential null value
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = await _context.Bookings
                .Where(b => b.RenterId == userId)  // Use user ID to retrieve their bookings
                .Include(b => b.Car)  // Include information about the car associated with the booking
                .Include(b => b.Renter)  // Include renter information
                .ToListAsync();

            return View(bookings);
        }

        // Show booking creation page
        public IActionResult Create(int carId)
        {
            var car = _context.Cars.Include(c => c.Owner).FirstOrDefault(c => c.Id == carId);
            if (car == null)
            {
                return NotFound();
            }

            var userId = User.Identity?.Name;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var renter = _context.Users.FirstOrDefault(u => u.UserName == userId);
            if (renter == null)
            {
                return NotFound("User not found");
            }

            // Set up new booking using car details
            var booking = new Booking
            {
                CarId = carId,
                TotalPrice = car.PricePerDay,  // Daily price of the car
                RenterId = userId,
                Car = car,
                Renter = renter,
                Status = BookingStatus.Pending
            };
            return View(booking);
        }

        // Process booking creation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                // Set RenterId to the current User (logged in user)
                var userId = User.Identity?.Name;
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                booking.RenterId = userId;

                // Update booking status to Pending when created
                booking.Status = BookingStatus.Pending;

                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyBookings));  // Redirect to user's bookings page
            }

            return View(booking);
        }

        // Display booking details
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

        // Update booking status to "Completed" or "Cancelled"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int bookingId, BookingStatus status)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            // Update booking status
            booking.Status = status;
            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyBookings));  // Redirect to user's bookings page
        }

        // Cancel booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            // Update status to "Cancelled"
            booking.Status = BookingStatus.Cancelled;
            _context.Update(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyBookings));
        }
    }
}
