using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System;

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

        // Display user's bookings page
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.Identity?.Name;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var bookings = await _context.Bookings
                                          .Include(b => b.Car)
                                          .Where(b => b.RenterId == userId)
                                          .ToListAsync();

            return View(bookings);
        }

        // Display payment creation page
        public async Task<IActionResult> Create(int bookingId)
        {
            var userId = User.Identity?.Name;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var booking = await _context.Bookings
                                         .Include(b => b.Car)
                                         .FirstOrDefaultAsync(b => b.Id == bookingId && b.RenterId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Car == null)
            {
                return NotFound("Car details not found");
            }

            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = booking.Car.PricePerDay * (booking.EndDate - booking.StartDate).Days,
                Status = Payment.PaymentStatus.Pending,
                PaymentMethod = "Credit Card",
                TransactionId = Guid.NewGuid().ToString()
            };

            return View(payment);
        }

        // Process payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity?.Name;
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                
                var booking = await _context.Bookings
                                             .FirstOrDefaultAsync(b => b.Id == payment.BookingId && b.RenterId == userId);

                if (booking == null)
                {
                    return NotFound();
                }

                booking.Status = BookingStatus.Confirmed;
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyBookings));
            }

            return View(payment);
        }
    }
}
