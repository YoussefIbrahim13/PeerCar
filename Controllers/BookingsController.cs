using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

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

        /// <summary>
        /// Displays all bookings for the current user.
        /// </summary>
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.RenterId == userId)
                .ToListAsync();

            return View(bookings);
        }

        /// <summary>
        /// Deletes a booking by its ID.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking == null)
                    return NotFound();

                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyBookings));
            }
            catch (Exception)
            {
                // Optionally inject ILogger<BookingsController> for logging
                // _logger.LogError(ex, $"Error deleting booking with ID {id}.");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the booking.");
                return RedirectToAction(nameof(MyBookings));
            }
        }

        /// <summary>
        /// Shows the booking creation page for a car.
        /// </summary>
        public async Task<IActionResult> Create(int carId)
        {
            var car = await _context.Cars.Include(c => c.Owner).FirstOrDefaultAsync(c => c.Id == carId);
            if (car == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var viewModel = new BookingViewModel
            {
                CarId = car.Id,
                RenterId = userId,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                TotalPrice = car.PricePerDay,
                CarBrand = car.Brand,
                CarModel = car.Model,
                PricePerDay = car.PricePerDay,
                OwnerUserName = car.Owner?.UserName,
                OwnerPhone = car.Owner?.PhoneNumber,
                CarImagePath = car.ImagePath,
                CarYear = car.Year,
                CarLocation = car.Location
            };
            return View(viewModel);
        }

        /// <summary>
        /// Processes booking creation.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingViewModel model, string paymentMethod)
        {
            var car = await _context.Cars.Include(c => c.Owner).FirstOrDefaultAsync(c => c.Id == model.CarId);
            var renter = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.RenterId);
            if (car == null)
            {
                ModelState.AddModelError("CarId", "Selected car does not exist.");
            }
            if (renter == null)
            {
                ModelState.AddModelError("RenterId", "Current user does not exist or is not logged in.");
            }
            if (model.StartDate > model.EndDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
            }
            // Server-side total price calculation
            decimal totalPrice = 0;
            if (car != null && model.StartDate != default && model.EndDate != default && model.EndDate >= model.StartDate)
            {
                var days = (model.EndDate - model.StartDate).Days + 1;
                totalPrice = days > 0 ? days * car.PricePerDay : car.PricePerDay;
            }
            // Check for overlapping bookings for this car (Confirmed or Pending)
            var overlapping = await _context.Bookings.AnyAsync(b =>
                b.CarId == model.CarId &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Completed &&
                (
                    (model.StartDate >= b.StartDate && model.StartDate <= b.EndDate) ||
                    (model.EndDate >= b.StartDate && model.EndDate <= b.EndDate) ||
                    (model.StartDate <= b.StartDate && model.EndDate >= b.EndDate)
                )
            );
            if (overlapping)
            {
                ModelState.AddModelError(string.Empty, "This car is already booked for the selected dates. Please choose different dates.");
            }
            if (ModelState.IsValid)
            {
                if (paymentMethod == "PayOnReceipt")
                {
                    var booking = new Booking
                    {
                        CarId = model.CarId,
                        RenterId = model.RenterId,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        TotalPrice = totalPrice,
                        Status = BookingStatus.Pending,
                        Car = car!,
                        Renter = renter!
                    };
                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(MyBookings));
                }
                else if (paymentMethod == "Card")
                {
                    TempData["CardPaymentMessage"] = "Card payment feature has not been added yet.";
                    // Repopulate car info for the view
                    if (car != null)
                    {
                        model.CarBrand = car.Brand;
                        model.CarModel = car.Model;
                        model.PricePerDay = car.PricePerDay;
                        model.OwnerUserName = car.Owner?.UserName;
                        model.CarImagePath = car.ImagePath;
                        model.CarYear = car.Year;
                        model.CarLocation = car.Location;
                        model.OwnerPhone = car.Owner?.PhoneNumber;
                    }
                    model.TotalPrice = totalPrice;
                    return View(model);
                }
            }
            // If invalid, repopulate car info for the view
            if (car != null)
            {
                model.CarBrand = car.Brand;
                model.CarModel = car.Model;
                model.PricePerDay = car.PricePerDay;
                model.OwnerUserName = car.Owner?.UserName;
                model.CarImagePath = car.ImagePath;
                model.CarYear = car.Year;
                model.CarLocation = car.Location;
                model.OwnerPhone = car.Owner?.PhoneNumber;
            }
            model.TotalPrice = totalPrice;
            ViewBag.FormError = "Please correct the highlighted errors and try again.";
            return View(model);
        }

        /// <summary>
        /// Internal method to handle booking creation logic.
        /// </summary>
        private async Task CreateBookingInternalAsync(Booking booking)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                throw new InvalidOperationException("User not logged in.");

            var car = await _context.Cars.Include(c => c.Owner).FirstOrDefaultAsync(c => c.Id == booking.CarId);
            var renter = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (car == null || renter == null)
                throw new InvalidOperationException("Car or renter not found.");

            booking.RenterId = userId;
            booking.Car = car;
            booking.Renter = renter;
            booking.Status = BookingStatus.Pending;
            _context.Add(booking);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Displays booking details.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Car)
                .ThenInclude(c => c.Owner)
                .Include(b => b.Renter)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        /// <summary>
        /// Updates booking status to Completed or Cancelled.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int bookingId, BookingStatus status)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null)
                {
                    return NotFound();
                }

                booking.Status = status;
                _context.Update(booking);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyBookings));
            }
            catch (Exception)
            {
                // _logger.LogError(ex, $"Error updating status for booking {bookingId}.");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the booking status.");
                return RedirectToAction(nameof(MyBookings));
            }
        }

        /// <summary>
        /// Cancels a booking by setting its status to Cancelled.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null)
                {
                    return NotFound();
                }

                booking.Status = BookingStatus.Cancelled;
                _context.Update(booking);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyBookings));
            }
            catch (Exception)
            {
                // _logger.LogError(ex, $"Error cancelling booking {bookingId}.");
                ModelState.AddModelError(string.Empty, "An error occurred while cancelling the booking.");
                return RedirectToAction(nameof(MyBookings));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCarReceived(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings.Include(b => b.Car).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null || booking.RenterId != userId)
                return Unauthorized();
            if (booking.Status != BookingStatus.Confirmed || booking.IsCarReceivedByUser)
                return BadRequest();
            booking.IsCarReceivedByUser = true;
            booking.CarReceivedDate = DateTime.Now;
            _context.Update(booking);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Car pickup confirmed.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCarReturned(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings.Include(b => b.Car).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null || booking.RenterId != userId)
                return Unauthorized();
            if (!booking.IsCarReceivedByUser || booking.IsCarReturnedByUser)
                return BadRequest();
            booking.IsCarReturnedByUser = true;
            booking.CarReturnedDate = DateTime.Now;
            // Early return/refund logic
            if (booking.CarReturnedDate.Value.Date < booking.EndDate.Date)
            {
                var unusedDays = (booking.EndDate.Date - booking.CarReturnedDate.Value.Date).Days;
                if (unusedDays > 0)
                {
                    booking.RefundAmount = unusedDays * booking.Car.PricePerDay;
                }
                booking.EndDate = booking.CarReturnedDate.Value.Date;
            }
            // Mark car as available
            booking.Car.AvailabilityStatus = Car.CarAvailabilityStatus.Available;
            _context.Update(booking);
            _context.Update(booking.Car);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Car return confirmed.";
            return RedirectToAction("Details", new { id });
        }
    }
}