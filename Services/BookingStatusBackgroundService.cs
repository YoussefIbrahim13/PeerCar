using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CarRentalMVC.Services
{
    public class BookingStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        public BookingStatusBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var now = DateTime.UtcNow;

                    // Automatically complete confirmed bookings whose end date has passed
                    var confirmedToCompleted = (await db.Bookings
                        .Where(b => b.Status == BookingStatus.Confirmed)
                        .ToListAsync(stoppingToken))
                        .Where(b => b.EndDate < now)
                        .ToList();
                    foreach (var booking in confirmedToCompleted)
                    {
                        booking.Status = BookingStatus.Completed;
                    }

                    // Cancel unapproved bookings after 1 day
                    var pendingBookings = (await db.Bookings
                        .Where(b => b.Status == BookingStatus.Pending)
                        .ToListAsync(stoppingToken))
                        .Where(b => (now - b.StartDate).TotalDays >= 1)
                        .ToList();
                    foreach (var booking in pendingBookings)
                    {
                        booking.Status = BookingStatus.Cancelled;
                    }

                    // Complete confirmed bookings 1 day after rental ends (legacy logic, can be removed if redundant)
                    var confirmedBookings = (await db.Bookings
                        .Where(b => b.Status == BookingStatus.Confirmed)
                        .ToListAsync(stoppingToken))
                        .Where(b => (now - b.EndDate).TotalDays >= 1)
                        .ToList();
                    foreach (var booking in confirmedBookings)
                    {
                        booking.Status = BookingStatus.Completed;
                    }

                    if (confirmedToCompleted.Any() || pendingBookings.Any() || confirmedBookings.Any())
                        await db.SaveChangesAsync(stoppingToken);
                }
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
