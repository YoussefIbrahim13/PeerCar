using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeerCar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeerCar.ViewComponents
{
    public class RecentReviewsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public RecentReviewsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 5)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Car)
                    .OrderByDescending(r => r.Date)
                    .Take(count)
                    .ToListAsync();

                // Convert to ReviewModels
                var reviewModels = reviews.Select(r => ReviewModel.FromReview(r)).ToList();

                return View(reviewModels);
            }
            catch (Exception)
            {
                // Return an empty list to avoid breaking the UI
                // Removed the unused ex variable
                return View(new List<ReviewModel>());
            }
        }
    }
}
