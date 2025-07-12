using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
namespace CarRentalMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Displays the admin dashboard with summary statistics and recent bookings.
        /// </summary>
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
            ViewBag.PendingSubmissions = await _context.CarSubmissions
                .Where(cs => cs.Status == CarSubmissionStatus.Pending)
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

        /// <summary>
        /// Displays the add user form.
        /// </summary>
        // GET: /admin/add-user
        [Route("add-user")]
        public IActionResult AddUser()
        {
            return View();
        }

        /// <summary>
        /// Handles the add user POST request.
        /// </summary>
        // POST: /admin/add-user
        [HttpPost]
        [Route("add-user")]
        public async Task<IActionResult> AddUser(string email, string password, string firstName, string lastName, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Name = $"{firstName} {lastName}",
                Status = UserStatus.Active
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                if (isAdmin)
                {
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                return RedirectToAction(nameof(Users));
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View();
        }

        /// <summary>
        /// Toggles the admin role for a user.
        /// </summary>
        // POST: /admin/users/toggle-admin
        [HttpPost]
        [Route("users/toggle-admin")]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            else
                await _userManager.AddToRoleAsync(user, "Admin");

            return RedirectToAction(nameof(Users));
        }

        /// <summary>
        /// Displays all bookings for admin.
        /// </summary>
        // GET: /admin/bookings
        [Route("bookings")]
        public async Task<IActionResult> Bookings(string status)
        {
            var query = _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Renter)
                .AsQueryable();
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, out var bookingStatus))
            {
                query = query.Where(b => b.Status == bookingStatus);
            }
            var bookings = await query.ToListAsync();
            return View(bookings);
        }

        /// <summary>
        /// Displays all users for admin.
        /// </summary>
        // GET: /admin/users
        [Route("users")]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        /// <summary>
        /// Displays all cars for admin.
        /// </summary>
        // GET: /admin/cars
        [Route("cars")]
        public async Task<IActionResult> Cars()
        {
            var cars = await _context.Cars.Include(c => c.Owner).ToListAsync();
            return View(cars);
        }

        /// <summary>
        /// Displays booking details for a specific booking.
        /// </summary>
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

        /// <summary>
        /// Updates the status of a booking.
        /// </summary>
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

        /// <summary>
        /// Updates the status of a user.
        /// </summary>
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

        /// <summary>
        /// Deletes a booking.
        /// </summary>
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

        /// <summary>
        /// Deletes a user.
        /// </summary>
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
            // Soft delete: set IsDeleted = true, sign out if currently logged in
            user.IsDeleted = true;
            // تحديث المراجعات المرتبطة بهذا المستخدم
            var reviews = await _context.Reviews.Where(r => r.UserId == user.Id).ToListAsync();
            foreach (var review in reviews)
            {
                review.UserId = null;
                review.AuthorName = "Deleted User";
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Users));
        }

        /// <summary>
        /// Deletes a car.
        /// </summary>
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

        /// <summary>
        /// Displays all users and their reservations for admin.
        /// </summary>
        // GET: /admin/userreservations
        [Route("userreservations")]
        public async Task<IActionResult> UserReservations()
        {
            // Filter out admin users from the list before displaying
            var adminRoleId = (await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin"))?.Id;
            var adminUserIds = adminRoleId != null ? await _context.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId).ToListAsync() : new List<string>();
            var users = await _context.Users
                .Where(u => !adminUserIds.Contains(u.Id))
                .Include(u => u.Bookings)
                .ThenInclude(b => b.Car)
                .ToListAsync();
            return View(users);
        }

        /// <summary>
        /// Updates the role for a user.
        /// </summary>
        [HttpPost]
        [Route("updateuserrole")]
        public async Task<IActionResult> UpdateUserRole(string userId, UserRole role)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();
            user.Role = role;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Users));
        }

        /// <summary>
        /// Deletes a user by email (case-insensitive).
        /// </summary>
        [HttpPost]
        [Route("delete-user-by-email")]
        public async Task<IActionResult> DeleteUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email required");
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());
            if (user == null) return NotFound();

            // Manually delete reviews where this user is the target
            var targetedReviews = await _context.Reviews
                .Where(r => r.TargetType == Review.ReviewTargetType.User && r.TargetId.ToString() == user.Id)
                .ToListAsync();
            if (targetedReviews.Any())
            {
                _context.Reviews.RemoveRange(targetedReviews);
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            return StatusCode(500, string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        /// <summary>
        /// Displays all deleted (soft-deleted) accounts and their related data (cars, bookings, reviews), supporting filtering and pagination.
        /// </summary>
        // GET: /admin/deleted-users
        [Route("deleted-users")]
        public async Task<IActionResult> DeletedUsers(int page = 1, int pageSize = 20, string search = null)
        {
            var query = _context.Users
                .Where(u => u.IsDeleted)
                .Include(u => u.OwnedCars)
                .Include(u => u.Bookings)
                .Include(u => u.Reviews)
                .AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => (u.Email ?? "").Contains(search) || (u.Name ?? "").Contains(search));
            }
            var total = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Search = search;
            return View(users);
        }

        /// <summary>
        /// Confirms that the refund for a booking has been processed.
        /// </summary>
        [HttpPost]
        [Route("bookings/confirm-refund")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmRefund(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();
            if (!booking.RefundAmount.HasValue || !booking.IsCarReturnedByUser)
                return BadRequest("Refund not applicable.");
            if (booking.IsRefunded)
                return BadRequest("Refund already confirmed.");
            booking.IsRefunded = true;
            _context.Update(booking);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Refund marked as processed.";
            return RedirectToAction("BookingDetails", new { id });
        }
    }
}
