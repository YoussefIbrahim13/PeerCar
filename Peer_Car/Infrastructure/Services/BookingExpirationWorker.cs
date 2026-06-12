using Microsoft.EntityFrameworkCore;
using Peer_Car.Domain.Enums;
using Peer_Car.Infrastructure.Data;

namespace Peer_Car.Infrastructure.Services
{
    public class BookingExpirationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public BookingExpirationWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var expiredBookings = await context.Bookings
                        .Where(b => b.Status == BookingStatus.Confirmed
                                 && b.StartDate < DateTime.UtcNow
                                 && !b.IsKeyHandedOver) 
                        .ToListAsync();

                    foreach (var booking in expiredBookings)
                    {
                        booking.Status = BookingStatus.Cancelled;
                    }

                    await context.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }
}
