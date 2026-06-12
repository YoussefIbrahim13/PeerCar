using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using PeerCar.Models;

namespace PeerCar.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(ApplicationDbContext context, ILogger<ReviewsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Displays all reviews.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Car)
                    .ToListAsync();
                // Convert to ReviewModel for the view
                var reviewModels = reviews.Select(r => ReviewModel.FromReview(r)).ToList();
                return View(reviewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviews.");
                ModelState.AddModelError(string.Empty, "Error loading reviews.");
                return View(new List<ReviewModel>());
            }
        }

        /// <summary>
        /// Displays a list of reviews, ordered by date.
        /// </summary>
        public async Task<IActionResult> ReviewList()
        {
            try
            {
                var reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Car)
                    .OrderByDescending(r => r.Date)
                    .ToListAsync();

                // Convert to ReviewModel
                var reviewModels = reviews.Select(r => ReviewModel.FromReview(r)).ToList();
                
                return View(reviewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading review list.");
                ModelState.AddModelError(string.Empty, "Error loading reviews.");
                return View(new List<ReviewModel>());
            }
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var review = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Car)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (review == null)
                {
                    return NotFound();
                }

                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review details for ID: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Reviews/Create
        public async Task<IActionResult> Create(int? carId, int? bookingId)
        {
            // Only allow review creation from a completed booking
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            ViewBag.TargetId = null;
            ViewBag.TargetName = null;
            ViewBag.BookingId = null;
            ViewBag.AlreadyReviewed = false;
            ViewBag.BookingStatus = null;

            if (!carId.HasValue || !bookingId.HasValue)
            {
                return View(new Review { UserId = userId }); // View will show error
            }

            var booking = await _context.Bookings.Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CarId == carId && b.RenterId == userId);
            if (booking == null)
            {
                return View(new Review { UserId = userId }); // View will show error
            }

            ViewBag.BookingId = booking.Id;
            ViewBag.TargetId = booking.CarId;
            ViewBag.TargetName = booking.Car != null ? $"{booking.Car.Brand} {booking.Car.Model}" : "Car";
            ViewBag.BookingStatus = booking.Status.ToString();

            // Only allow review if booking is completed
            if (booking.Status != BookingStatus.Completed)
            {
                return View(new Review { UserId = userId }); // View will show error
            }

            // Only allow one review per booking per user
            var alreadyReviewed = await _context.Reviews.AnyAsync(r => r.BookingId == booking.Id && r.UserId == userId);
            ViewBag.AlreadyReviewed = alreadyReviewed;
            if (alreadyReviewed)
            {
                return View(new Review { UserId = userId }); // View will show error
            }

            var model = new Review
            {
                TargetId = booking.CarId,
                TargetType = Review.ReviewTargetType.Car,
                BookingId = booking.Id,
                UserId = userId
            };
            return View(model);
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TargetId,TargetType,Comment,BookingId")] Review review)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var booking = await _context.Bookings.Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.Id == review.BookingId && b.CarId == review.TargetId && b.RenterId == userId);
            if (booking == null || booking.Status != BookingStatus.Completed)
            {
                ModelState.AddModelError(string.Empty, "Invalid booking or booking not completed.");
            }
            var alreadyReviewed = await _context.Reviews.AnyAsync(r => r.BookingId == review.BookingId && r.UserId == userId);
            if (alreadyReviewed)
            {
                ModelState.AddModelError(string.Empty, "You have already submitted a review for this booking.");
            }
            // Assign UserId before validation
            review.UserId = userId;
            // Update ModelState for UserId so validation uses the assigned value
            ModelState.Remove(nameof(review.UserId));
            review.Date = DateTime.Now;
            review.TargetType = Review.ReviewTargetType.Car;
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Saving review: {@Review}", review);
                    _context.Add(review);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Cars", new { id = review.TargetId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating review.");
                    ModelState.AddModelError(string.Empty, "Error creating review.");
                }
            }
            else
            {
                // Log ModelState errors for debugging
                foreach (var key in ModelState.Keys)
                {
                    var entry = ModelState[key];
                    if (entry != null && entry.Errors != null)
                    {
                        foreach (var error in entry.Errors)
                        {
                            _logger.LogWarning("ModelState error for {Key}: {Error}", key, error.ErrorMessage);
                        }
                    }
                }
            }
            // Repopulate viewbag for error display
            ViewBag.TargetId = review.TargetId;
            var car = await _context.Cars.FindAsync(review.TargetId);
            ViewBag.TargetName = car != null ? $"{car.Brand} {car.Model}" : "Car";
            ViewBag.BookingId = review.BookingId;
            ViewBag.BookingStatus = booking?.Status.ToString();
            ViewBag.AlreadyReviewed = alreadyReviewed;
            return View(review);
        }

        // GET: Reviews/Edit/5
        [Authorize]
        public IActionResult Edit(int? id)
        {
            return Forbid(); // Editing reviews is not allowed
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Edit(int id, [Bind("Id,TargetId,TargetType,Comment")] Review review)
        {
            return Forbid(); // Editing reviews is not allowed
        }

        // GET: Reviews/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }
            if (id == null)
            {
                return NotFound();
            }
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Car)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null)
            {
                return NotFound();
            }
            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Reviews for a specific car
        public async Task<IActionResult> CarReviews(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.TargetType == Review.ReviewTargetType.Car && r.TargetId == id)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            ViewBag.Car = car;
            ViewBag.CanReview = User.Identity != null && User.Identity.IsAuthenticated;

            return View(reviews);
        }

        // GET: Reviews by a specific user
        public async Task<IActionResult> UserReviews(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .Include(r => r.Car)
                .Where(r => r.UserId == id)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            ViewBag.User = user;
            return View(reviews);
        }

        // GET: Reviews about a specific user
        public async Task<IActionResult> AboutUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.TargetType == Review.ReviewTargetType.User && r.TargetId.ToString() == id)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.CanReview = User.Identity != null && User.Identity.IsAuthenticated && User.FindFirstValue(ClaimTypes.NameIdentifier) != id;

            return View(reviews);
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
}
