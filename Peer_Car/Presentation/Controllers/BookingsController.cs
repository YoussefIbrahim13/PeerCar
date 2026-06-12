using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Peer_Car.Application.Interfaces;
using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;
using System.Security.Claims;

namespace Peer_Car.Presentation.Controllers
{
    [Authorize]
    [Route("bookings")]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IFileService _fileService;

        public BookingsController(
            IBookingService bookingService,
            ILogger<BookingsController> logger,
            UserManager<User> userManager,
            IFileService fileService)
        {
            _bookingService = bookingService;
            _logger = logger;
            _userManager = userManager;
            _fileService = fileService;
        }

        #region Basic Actions (Read/Create/Delete)

        [HttpGet("my-bookings")]
        public async Task<IActionResult> MyBookings()
        {
            var userId = GetUserId();
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return View(bookings);
        }

        [HttpGet("create/{carId:guid}")]
        public async Task<IActionResult> Create(Guid carId)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId.ToString());

            var model = await _bookingService.PrepareBookingViewModelAsync(carId, userId);
            if (model == null) return NotFound();

            model.RequiresDocuments = string.IsNullOrEmpty(user?.NationalIdFrontUrl);

            return View(model);
        }
        [HttpPost("create/{carId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid carId, BookingViewModel viewModel, string paymentMethod)
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null) return Challenge();

            if (user.DocumentStatus != DocumentStatus.Verified)
            {
                if (user.DocumentStatus == DocumentStatus.NotUploaded || user.DocumentStatus == DocumentStatus.Rejected)
                {
                    if (viewModel.IdFrontFile == null || viewModel.IdBackFile == null ||
                        viewModel.LicenseFrontFile == null || viewModel.LicenseBackFile == null)
                    {
                        ModelState.AddModelError("", "For security, please upload all required documents.");
                        return View(viewModel);
                    }

                    user.NationalIdFrontUrl = await _fileService.SaveDocumentAsync(viewModel.IdFrontFile, "NationalID");
                    user.NationalIdBackUrl = await _fileService.SaveDocumentAsync(viewModel.IdBackFile, "NationalID");
                    user.LicenseFrontUrl = await _fileService.SaveDocumentAsync(viewModel.LicenseFrontFile, "License");
                    user.LicenseBackUrl = await _fileService.SaveDocumentAsync(viewModel.LicenseBackFile, "License");

                    user.DocumentStatus = DocumentStatus.Pending;
                    user.IsDocumentsVerified = false;

                    var updateResult = await _userManager.UpdateAsync(user);

                    if (!updateResult.Succeeded)
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(viewModel);
                    }

                    TempData["InfoMessage"] = "Documents uploaded! Waiting for admin approval.";
                    return RedirectToAction("MyAccount", "Account");
                }

                if (user.DocumentStatus == DocumentStatus.Pending)
                {
                    TempData["InfoMessage"] = "Your documents are still under review.";
                    return RedirectToAction("MyAccount", "Account");
                }
            }

            viewModel.CarId = carId;
            viewModel.RenterId = userId;

            if (!ModelState.IsValid) return View(viewModel);

            var result = await _bookingService.CreateBookingAsync(viewModel, paymentMethod);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Booking request submitted successfully!";
                return RedirectToAction(nameof(MyBookings));
            }
            else
            {
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(viewModel);
            }
        }

        [HttpGet("details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var booking = await _bookingService.GetBookingDetailsAsync(id);
            if (booking == null) return NotFound();

            var currentUserId = GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isRenter = booking.RenterId == currentUserId;
            bool isOwner = booking.Car.OwnerId == currentUserId;

            if (!isAdmin && !isRenter && !isOwner) return Forbid();

            return View(booking);
        }

        #endregion

        #region Management Actions (Confirm/Cancel/Finalize)

        [HttpPost("ConfirmCarReceived/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPickup(Guid id)
        {
            var success = await _bookingService.ConfirmPickupAsync(id, GetUserId());
            if (success) TempData["SuccessMessage"] = "Handover confirmed successfully!";
            else TempData["ErrorMessage"] = "Unable to confirm handover.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("confirm-return/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReturn(Guid id)
        {
            var success = await _bookingService.ConfirmReturnAsync(id, GetUserId());
            if (success) TempData["SuccessMessage"] = "Car return confirmed. Trip completed successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("approve/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBooking(Guid id)
        {
            var success = await _bookingService.ApproveBookingAsync(id, GetUserId());
            if (success) TempData["SuccessMessage"] = "Booking approved successfully!";
            else TempData["ErrorMessage"] = "You are not authorized or booking status is invalid.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("RequestReturn/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestReturn(Guid id)
        {
            var success = await _bookingService.RequestReturnAsync(id, GetUserId());
            if (success) TempData["SuccessMessage"] = "Return request sent! Waiting for owner confirmation.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("FinalizeBooking/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeBooking(Guid id)
        {
            var success = await _bookingService.FinalizeBookingAsync(id, GetUserId());
            if (success) TempData["SuccessMessage"] = "Booking finalized! The car is now available again.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("cancel/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var success = await _bookingService.CancelBookingAsync(id, GetUserId());
            if (success) TempData["SuccessMessage"] = "Booking cancelled successfully.";
            return RedirectToAction(nameof(MyBookings));
        }

        #endregion

        #region Owner Views

        [HttpGet("my-cars-bookings")]
        public async Task<IActionResult> MyCarsBookings()
        {
            var carsWithBookings = await _bookingService.GetOwnerCarsWithBookingsAsync(GetUserId());
            return View(carsWithBookings);
        }

        #endregion

        private Guid GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim != null ? Guid.Parse(claim) : Guid.Empty;
        }
    }
}