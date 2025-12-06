using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentalMVC.Models;
using CarRentalMVC.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CarRentalMVC.ViewComponents
{
    public class RecentCarsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public RecentCarsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 3)
        {
            var cars = await _context.Cars
                .Where(c => c.SubmissionStatus == CarSubmissionStatus.Approved)
                .OrderByDescending(c => c.Id)
                .Take(count)
                .ToListAsync();
            return View(cars);
        }
    }
}
