using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Peer_Car.Application.Interfaces;
using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Enums;
using Peer_Car.Domain.Interfaces;
using System.Security.Claims;

namespace Peer_Car.Presentation.Controllers
{



    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;
        private readonly ICarSubmissionService _submissionService;
        public AdminController(IAdminService adminService, ICarSubmissionService submissionService, ILogger<AdminController> logger)
        {
            _submissionService = submissionService;
            _adminService = adminService;
            _logger = logger;
        }

        #region Dashboard
        [Route("")]
        [Route("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var stats = await _adminService.GetDashboardDataAsync();
            return View(stats);
        }
        #endregion

        #region User Management

        [HttpGet("users")]
        public async Task<IActionResult> Users()
        {
            var users = await _adminService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet("add-user")]
        public IActionResult AddUser() => View();

        [HttpPost("add-user")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _adminService.CreateUserAsync(model);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        [HttpPost("users/update-role")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(Guid userId, UserRole role)
        {
            if (userId == Guid.Empty) return BadRequest();

            await _adminService.UpdateUserRoleAsync(userId, role);
            TempData["SuccessMessage"] = "User role updated successfully.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost("users/toggle-admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdmin(Guid userId)
        {
            if (userId == Guid.Empty) return BadRequest();

            var result = await _adminService.ToggleAdminRoleAsync(userId);
            if (result.Succeeded) TempData["SuccessMessage"] = "Admin status toggled.";

            return RedirectToAction(nameof(Users));
        }


        [HttpPost("users/update-status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserStatus(Guid userId, UserStatus status)
        {
            if (userId == Guid.Empty) return BadRequest();

            await _adminService.UpdateUserStatusAsync(userId, status);
            return RedirectToAction(nameof(Users));
        }

        [HttpPost("users/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            if (userId == Guid.Empty) return BadRequest();

            await _adminService.SoftDeleteUserAsync(userId);
            TempData["SuccessMessage"] = "User has been soft-deleted.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost("users/delete-by-email")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Email is required.");

            var success = await _adminService.DeleteUserByEmailAsync(email);
            if (success) TempData["SuccessMessage"] = "User deleted permanently.";
            else TempData["ErrorMessage"] = "User not found or could not be deleted.";

            return RedirectToAction(nameof(Users));
        }

        #endregion

        #region Specialized User Queries

        [HttpGet("user-reservations")]
        public async Task<IActionResult> UserReservations()
        {
            var users = await _adminService.GetUsersWithReservationsAsync();
            return View(users);
        }

        [HttpGet("deleted-users")]
        public async Task<IActionResult> DeletedUsers(int page = 1, int pageSize = 20, string? search = null)
        {
            var (users, total) = await _adminService.GetDeletedUsersAsync(page, pageSize, search ?? "");
            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.Search = search;
            return View(users);
        }

        #endregion

        #region Car Management

        [HttpGet("cars")]
        public async Task<IActionResult> Cars()
        {
            var cars = await _adminService.GetAllCarsAsync();
            return View(cars);
        }

        [HttpPost("cars/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCar(Guid carId)
        {
            if (carId == Guid.Empty) return BadRequest();

            await _adminService.DeleteCarAsync(carId);
            return RedirectToAction(nameof(Cars));
        }

        #endregion

        #region Booking Management

        [HttpGet("bookings")]
        public async Task<IActionResult> Bookings(string? status)
        {
            BookingStatus? bookingStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, out var parsedStatus))
            {
                bookingStatus = parsedStatus;
            }

            var bookings = await _adminService.GetBookingsAsync(bookingStatus);
            return View(bookings);
        }

        [HttpGet("bookings/{id:guid}")]
        public async Task<IActionResult> BookingDetails(Guid id)
        {
            var booking = await _adminService.GetBookingDetailsAsync(id);
            if (booking == null) return NotFound();
            return View(booking);
        }




        #endregion
        #region Car Submissions (Review Process)

        [HttpGet("submissions")]
        public async Task<IActionResult> CarSubmissions()
        {
            var groupedSubmissions = await _submissionService.GetGroupedPendingSubmissionsAsync();
            return View(groupedSubmissions);
        }

        [HttpGet("submissions/{id:guid}")]
        public async Task<IActionResult> SubmissionDetails(Guid id)
        {
            var submission = await _submissionService.GetSubmissionDetailsAsync(id);
            if (submission == null) return NotFound();

            return View(submission);
        }

        [HttpPost("submissions/approve/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveSubmission(Guid id)
        {
            if (id == Guid.Empty) return BadRequest();

            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _submissionService.ApproveSubmissionAsync(id, adminId);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Car has been approved and is now live!";

                return RedirectToAction(nameof(Cars));
            }

            return BadRequest("Failed to approve submission.");
        }

        [HttpPost("submissions/reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectSubmission(Guid id, string reason)
        {
            if (id == Guid.Empty || string.IsNullOrEmpty(reason))
            {
                TempData["ErrorMessage"] = "Rejection reason is required.";
                return RedirectToAction(nameof(SubmissionDetails), new { id });
            }

            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _submissionService.RejectSubmissionAsync(id, reason, adminId);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Car submission rejected.";
                return RedirectToAction(nameof(CarSubmissions));
            }

            return BadRequest("Failed to reject submission.");
        }

        #endregion


        [HttpGet("user-details/{id:guid}")]
        public async Task<IActionResult> UserDetails(Guid id)
        {
            var user = await _adminService.GetUserDetailsAsync(id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found!";
                return RedirectToAction(nameof(Users));
            }

            return View(user);
        }
        [HttpGet("verify-users")]
        public async Task<IActionResult> VerifyUsers()
        {
            var pendingUsers = await _adminService.GetUsersPendingVerificationAsync();
            return View(pendingUsers);
        }

        [HttpPost("approve-user/{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(Guid userId)
        {
            await _adminService.ProcessUserVerificationAsync(userId, true);
            TempData["SuccessMessage"] = "User identity verified successfully.";
            return RedirectToAction(nameof(VerifyUsers));
        }

        [HttpPost("reject-user/{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectUser(Guid userId)
        {
            await _adminService.ProcessUserVerificationAsync(userId, false);
            TempData["ErrorMessage"] = "User identity rejected.";
            return RedirectToAction(nameof(VerifyUsers));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("archive")]
        public async Task<IActionResult> Archive()
        {
            var model = new AdminArchiveViewModel
            {
                DeletedUsers = await _adminService.GetDeletedUsersAsync(),
                DeletedCars = await _adminService.GetDeletedCarsAsync(),
                DeletedBookings = await _adminService.GetDeletedBookingsAsync()
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("pending-verifications")]
        public async Task<IActionResult> PendingVerifications()
        {
            var pendingUsers = await _adminService.GetUsersPendingVerificationAsync();
            return View("PendingVerifications", pendingUsers);
        }
    }
}