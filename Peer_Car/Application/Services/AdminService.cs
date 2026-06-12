using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Peer_Car.Application.Interfaces;
using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;
using Peer_Car.Infrastructure.Data;

namespace Peer_Car.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IEmailSender _emailSender;
        public AdminService(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        public async Task<AdminDashboardViewModel> GetDashboardDataAsync()
        {
            return new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(u => !u.IsDeleted),
                TotalCars = await _context.Cars.CountAsync(),
                TotalBookings = await _context.Bookings.CountAsync(),
                PendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending),
                PendingSubmissions = await _context.CarSubmissions.CountAsync(cs => cs.Status == CarSubmissionStatus.Pending),
                RecentBookings = await _context.Bookings
                    .Include(b => b.Car).Include(b => b.Renter)
                    .OrderByDescending(b => b.CreatedAt).Take(5).ToListAsync()
            };
        }

        public async Task<IdentityResult> CreateUserAsync(CreateUserViewModel model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Status = UserStatus.Active,
                DateRegistered = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded && model.IsAdmin)
            {
                await EnsureRoleExists("Admin");
                await _userManager.AddToRoleAsync(user, "Admin");
                user.Role = UserRole.Admin;
                await _userManager.UpdateAsync(user);
            }

            return result;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            // بنجيب اليوزرز اللي مش ممسوحين سوفت ديليت
            return await _context.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.DateRegistered)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingDetailsAsync(Guid id)
        {
            return await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Renter)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IdentityResult> ToggleAdminRoleAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            await EnsureRoleExists("Admin");
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                user.Role = UserRole.Renter; // بنرجعه يوزر عادي في الـ Enum برضه
                await _userManager.UpdateAsync(user);
                return await _userManager.RemoveFromRoleAsync(user, "Admin");
            }

            user.Role = UserRole.Admin;
            await _userManager.UpdateAsync(user);
            return await _userManager.AddToRoleAsync(user, "Admin");
        }

        public async Task SoftDeleteUserAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {

                user.IsDeleted = true;

                string suffix = DateTime.UtcNow.Ticks.ToString();

                user.Email = $"deleted_{suffix}_{user.Email}";
                user.UserName = $"deleted_{suffix}_{user.UserName}";
                user.NormalizedEmail = user.Email.ToUpper();
                user.NormalizedUserName = user.UserName.ToUpper();

                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    user.PhoneNumber = $"del_{suffix}_{user.PhoneNumber}";
                }


                var reviews = await _context.Reviews.Where(r => r.UserId == userId).ToListAsync();
                foreach (var r in reviews)
                {

                    r.AuthorName = "Deleted User";
                }

                await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateBookingStatusAsync(Guid id, BookingStatus status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetDeletedUsersAsync(int page, int pageSize, string search)
        {
            var query = _context.Users.Where(u => u.IsDeleted)
                .Include(u => u.OwnedCars).Include(u => u.Bookings).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.Email.Contains(search) || u.FirstName.Contains(search));

            int total = await query.CountAsync();
            var users = await query.OrderByDescending(u => u.DateRegistered)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (users, total);
        }

        public async Task DeleteCarAsync(Guid carId)
        {

            var car = await _context.Cars
                .Include(c => c.Bookings)
                .FirstOrDefaultAsync(c => c.Id == carId);

            if (car != null)
            {

                car.IsDeleted = true;


                var pendingBookings = await _context.Bookings
               .Include(b => b.Renter)
               .Where(b => b.CarId == carId && b.Status == BookingStatus.Pending)
               .ToListAsync();

                foreach (var booking in pendingBookings)
                {
                    booking.Status = BookingStatus.Cancelled;
                    var subject = "Booking Cancellation - PeerCar";
                    var message = $@"
                <h3>Hello {booking.Renter.FirstName},</h3>
                <p>We regret to inform you that your pending booking for <strong>{car.Brand} {car.Model}</strong> has been cancelled.</p>
                <p>Reason: The car is no longer available on our platform.</p>
                <p>If you have already paid, a refund will be processed shortly.</p>
                <p>Best regards,<br>PeerCar Team</p>";

                    await _emailSender.SendEmailAsync(booking.Renter.Email!, subject, message);
                }


                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Car>> GetAllCarsAsync() =>
            await _context.Cars.Include(c => c.Owner).ToListAsync();

        public async Task UpdateUserStatusAsync(Guid userId, UserStatus status)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Status = status;
                user.LastActive = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateUserRoleAsync(Guid userId, UserRole role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return;

            user.Role = role;
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            string roleName = role.ToString();
            await EnsureRoleExists(roleName);
            await _userManager.AddToRoleAsync(user, roleName);

            await _userManager.UpdateAsync(user);
        }

        public async Task<bool> DeleteUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<IEnumerable<Booking>> GetBookingsAsync(BookingStatus? status)
        {
            var query = _context.Bookings.Include(b => b.Car).Include(b => b.Renter).AsQueryable();
            if (status.HasValue) query = query.Where(b => b.Status == status);
            return await query.ToListAsync();
        }

        public async Task DeleteBookingAsync(Guid id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ConfirmRefundAsync(Guid id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null || booking.IsRefunded || !booking.RefundAmount.HasValue) return false;

            booking.IsRefunded = true;
            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<User>> GetUsersWithReservationsAsync()
        {
            return await _context.Users
                .Include(u => u.Bookings).ThenInclude(b => b.Car)
                .Where(u => u.Bookings.Any())
                .ToListAsync();
        }

        private async Task EnsureRoleExists(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }


        public async Task<User?> GetUserDetailsAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.OwnedCars)
                .Include(u => u.Bookings)
                    .ThenInclude(b => b.Car)
                .FirstOrDefaultAsync(u => u.Id == id);
        }



        public async Task<IEnumerable<User>> GetUsersPendingVerificationAsync()
        {
            return await _context.Users
                .Where(u => u.DocumentStatus == DocumentStatus.Pending)
                .OrderBy(u => u.DateRegistered)
                .ToListAsync();
        }

        public async Task<bool> ProcessUserVerificationAsync(Guid userId, bool approve)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (approve)
            {
                user.DocumentStatus = DocumentStatus.Verified;
                user.IsDocumentsVerified = true;
            }
            else
            {
                user.DocumentStatus = DocumentStatus.Rejected;
                user.IsDocumentsVerified = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }


        // جلب كل المستخدمين المحذوفين (Soft Deleted)
        public async Task<IEnumerable<User>> GetDeletedUsersAsync()
        {
            return await _context.Users
                .IgnoreQueryFilters() // تجاهل الفلتر اللي بيخفي IsDeleted
                .Where(u => u.IsDeleted)
                .OrderByDescending(u => u.DateRegistered)
                .ToListAsync();
        }

        // جلب كل العربيات المحذوفة
        public async Task<IEnumerable<Car>> GetDeletedCarsAsync()
        {
            return await _context.Cars
                .IgnoreQueryFilters()
                .Where(c => c.IsDeleted)
                .Include(c => c.Owner) // عشان نعرف مين صاحب العربية اللي اتمسحت
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }

        // جلب كل الحجوزات المحذوفة (أو اللي تخص عربيات/يوزرز ممسوحين)
        public async Task<IEnumerable<Booking>> GetDeletedBookingsAsync()
        {
            return await _context.Bookings
                .IgnoreQueryFilters()
                .Include(b => b.Car)
                .Include(b => b.Renter)
                .Where(b => b.Status == BookingStatus.Cancelled || b.Car.IsDeleted || b.Renter.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }


        public async Task<IEnumerable<User>> GetPendingVerificationsAsync()
        {
            return await _context.Users
                .Where(u => u.DocumentStatus == DocumentStatus.Pending)
                .OrderBy(u => u.DateRegistered)
                .ToListAsync();
        }

        public async Task<bool> ProcessUserDocumentsAsync(Guid userId, bool isApproved)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (isApproved)
            {
                user.DocumentStatus = DocumentStatus.Verified;
                user.IsDocumentsVerified = true;
            }
            else
            {
                user.DocumentStatus = DocumentStatus.Rejected;
                user.IsDocumentsVerified = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }



}

