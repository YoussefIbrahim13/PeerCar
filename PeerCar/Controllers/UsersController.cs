using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentalMVC.Data;
using CarRentalMVC.Models;

namespace CarRentalMVC.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users/Profile/{id}
        public async Task<IActionResult> Profile(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _context.Users
                .Include(u => u.OwnedCars)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var model = new UserProfileViewModel
            {
                FullName = user.Name,
                ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImageUrl) ? null : System.IO.Path.GetFileName(user.ProfileImageUrl),
                PhoneNumber = user.PhoneNumber,
                Cars = user.OwnedCars.ToList(),
                Role = user.Role.ToString(),
                AverageRating = null, // تم حذف التقييم
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed
            };
            return View(model);
        }
    }
}
