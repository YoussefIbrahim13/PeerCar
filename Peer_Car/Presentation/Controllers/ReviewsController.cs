using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Peer_Car.Application.Interfaces;
using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;
using System.Security.Claims;

namespace Peer_Car.Presentation.Controllers
{
    [Authorize]
    [Route("reviews")]
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        #region Basic Views

        // 1. استدعاء GetAllReviewsAsync (لعرض كل المراجعات في السيستم)
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            return View(reviews);
        }

        // 2. استدعاء GetReviewByIdAsync (لعرض تفاصيل مراجعة معينة)
        [HttpGet("details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null) return NotFound();
            return View(review);
        }

        #endregion

        #region Create Review Logic

        // 1. دي الميثود اللي بتفتح الصفحة لما تدوس على الزرار (GET)
        [HttpGet("create/{bookingId:guid}")]
        public async Task<IActionResult> Create(Guid bookingId)
        {
            var userId = GetUserId();

            if (bookingId == Guid.Empty) return RedirectToAction("MyBookings", "Bookings");

            // بنسأل السيرفس: هل اليوزر ده مسموح له يقيم الحجز ده؟
            var validation = await _reviewService.ValidateReviewEligibilityAsync(bookingId, userId);

            if (!validation.canReview)
            {
                TempData["ErrorMessage"] = validation.message;
                return RedirectToAction("MyBookings", "Bookings");
            }

            // 💡 مهم جداً: بنملا الـ ViewBag بالبيانات اللي الـ View مستنيها عشان مايطلعش "Oops!"
            ViewBag.TargetId = validation.carId;
            ViewBag.BookingId = bookingId;
            ViewBag.TargetName = validation.carName;
            ViewBag.TargetImage = validation.carImagePath;

            var model = new Review
            {
                BookingId = bookingId,
                TargetId = validation.carId ?? Guid.Empty,
                TargetType = ReviewTargetType.Car,
                UserId = userId
            };

            return View(model);
        }

        // 2. دي الميثود اللي بتسيف التقييم لما تدوس Submit (POST)
        [HttpPost("create/{bookingId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromRoute] Guid bookingId, Review review)
        {
            // 💡 التعديل هنا: بنشيل الـ UserId من الـ Validation لأنه بيتحط يدوياً تحت
            ModelState.Remove("UserId");
            ModelState.Remove("User"); // لو عندك Navigation Property

            if (!ModelState.IsValid)
            {
                // لو لسه فيه أخطاء، ارجع إملا الـ ViewBags عشان الصفحة متضربش "Oops!"
                var userId = GetUserId();
                var validation = await _reviewService.ValidateReviewEligibilityAsync(bookingId, userId);

                ViewBag.TargetId = validation.carId;
                ViewBag.BookingId = bookingId;
                ViewBag.TargetName = validation.carName;
                ViewBag.TargetImage = validation.carImagePath;

                return View(review);
            }

            try
            {
                review.UserId = GetUserId();
                review.BookingId = bookingId;
                review.CreatedAt = DateTime.UtcNow; // تأكد من التاريخ

                await _reviewService.CreateReviewAsync(review);

                TempData["SuccessMessage"] = "Thank you! Your review has been posted.";
                return RedirectToAction("Details", "Cars", new { id = review.TargetId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                ModelState.AddModelError("", "Internal server error. Please try again.");
                return View(review);
            }
        }

        #endregion

        #region Specialized Filters (Specialized Queries)

        [HttpGet("CarReviews/{carId:guid}")] 
        public async Task<IActionResult> CarReviews(Guid carId)
        {
            var reviews = await _reviewService.GetReviewsByCarIdAsync(carId);
            return View(reviews);
        }

        [HttpGet("my-reviews")]
        public async Task<IActionResult> MyReviews()
        {
            var reviews = await _reviewService.GetReviewsByUserAsync(GetUserId());
            return View(reviews);
        }

        [HttpGet("about-user/{userId:guid}")]
        public async Task<IActionResult> AboutUser(Guid userId)
        {
            var reviews = await _reviewService.GetReviewsAboutUserAsync(userId);
            return View(reviews);
        }

        #endregion

        #region Admin Operations

        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _reviewService.DeleteReviewAsync(id);
            TempData["SuccessMessage"] = "Review deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        private Guid GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim != null ? Guid.Parse(claim) : Guid.Empty;
        }
    }
}
