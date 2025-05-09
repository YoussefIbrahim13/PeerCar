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

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            try
            {
                var reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Car)
                    .ToListAsync();
                return View(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviews.");
                ModelState.AddModelError(string.Empty, "Error loading reviews.");
                return View(new List<Review>());
            }
        }

        // GET: Reviews/ReviewList
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
        public IActionResult Create(int? targetId, string targetType)
        {
            // Validate that target exists
            if (targetId.HasValue)
            {
                if (targetType == "Car")
                {
                    var car = _context.Cars.Find(targetId);
                    if (car == null)
                    {
                        return NotFound();
                    }
                    ViewBag.TargetName = $"{car.Brand} {car.Model}";
                }
                else if (targetType == "User")
                {
                    var user = _context.Users.Find(targetId);
                    if (user == null)
                    {
                        return NotFound();
                    }
                    ViewBag.TargetName = user.UserName;
                }
                else
                {
                    return BadRequest();
                }

                ViewBag.TargetId = targetId;
                ViewBag.TargetType = targetType;
            }
            else
            {
                // If no target is specified, offer a dropdown to select
                ViewBag.Cars = new SelectList(_context.Cars, "Id", "Brand");
                ViewBag.Users = new SelectList(_context.Users, "Id", "UserName");
                ViewBag.TargetTypes = new SelectList(Enum.GetValues(typeof(Review.ReviewTargetType))
                                      .Cast<Review.ReviewTargetType>(), "Value", "Text");
            }

            return View();
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TargetId,TargetType,Rating,Comment")] Review review)
        {
            // Set the current user as the reviewer
            review.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            review.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(review);
                    await _context.SaveChangesAsync();

                    // Redirect based on target type
                    if (review.TargetType == Review.ReviewTargetType.Car)
                    {
                        return RedirectToAction("Details", "Cars", new { id = review.TargetId });
                    }
                    else
                    {
                        // Could redirect to user profile in the future
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating review.");
                    ModelState.AddModelError(string.Empty, "Error creating review.");
                }
            }

            // If we get here, something went wrong
            if (review.TargetType == Review.ReviewTargetType.Car)
            {
                var car = await _context.Cars.FindAsync(review.TargetId);
                if (car != null)
                {
                    ViewBag.TargetName = $"{car.Brand} {car.Model}";
                }
            }
            else
            {
                var user = await _context.Users.FindAsync(review.TargetId);
                if (user != null)
                {
                    ViewBag.TargetName = user.UserName;
                }
            }

            ViewBag.TargetId = review.TargetId;
            ViewBag.TargetType = review.TargetType;

            return View(review);
        }

        // GET: Reviews/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            // Only allow editing by the review author or admins
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (review.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Add target name to ViewBag
            if (review.TargetType == Review.ReviewTargetType.Car)
            {
                var car = await _context.Cars.FindAsync(review.TargetId);
                if (car != null)
                {
                    ViewBag.TargetName = $"{car.Brand} {car.Model}";
                }
            }
            else
            {
                var user = await _context.Users.FindAsync(review.TargetId);
                if (user != null)
                {
                    ViewBag.TargetName = user.UserName;
                }
            }

            return View(review);
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TargetId,TargetType,Rating,Comment")] Review review)
        {
            if (id != review.Id)
            {
                return NotFound();
            }

            // Get the original review to verify ownership
            var originalReview = await _context.Reviews.FindAsync(id);
            if (originalReview == null)
            {
                return NotFound();
            }

            // Only allow editing by the review author or admins
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (originalReview.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Keep original values that shouldn't change
            review.UserId = originalReview.UserId;
            review.Date = originalReview.Date;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ReviewExists(review.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error updating review ID: {Id}", id);
                        ModelState.AddModelError(string.Empty, "Concurrency error. The review was modified by another user.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating review ID: {Id}", id);
                    ModelState.AddModelError(string.Empty, "Error updating review.");
                }
            }

            // If we get here, something went wrong
            if (review.TargetType == Review.ReviewTargetType.Car)
            {
                var car = await _context.Cars.FindAsync(review.TargetId);
                if (car != null)
                {
                    ViewBag.TargetName = $"{car.Brand} {car.Model}";
                }
            }
            else
            {
                var user = await _context.Users.FindAsync(review.TargetId);
                if (user != null)
                {
                    ViewBag.TargetName = user.UserName;
                }
            }

            return View(review);
        }

        // GET: Reviews/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
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

            // Only allow deletion by the review author or admins
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (review.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            // Only allow deletion by the review author or admins
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (review.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review ID: {Id}", id);
                ModelState.AddModelError(string.Empty, "Error deleting review.");
                return RedirectToAction(nameof(Index));
            }
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
            ViewBag.CanReview = User.Identity.IsAuthenticated;

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
            ViewBag.CanReview = User.Identity.IsAuthenticated && User.FindFirstValue(ClaimTypes.NameIdentifier) != id;

            return View(reviews);
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
}
