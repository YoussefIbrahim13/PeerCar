using Microsoft.EntityFrameworkCore;
using Peer_Car.Domain.Enums;
using Peer_Car.Infrastructure.Data;

namespace Peer_Car.Infrastructure.Services
{
    public class BookingStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingStatusBackgroundService> _logger;

        public BookingStatusBackgroundService(IServiceProvider serviceProvider, ILogger<BookingStatusBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Status Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var now = DateTime.UtcNow;

                        // 1. تحويل الحجوزات المؤكدة إلى "مكتملة" إذا انتهى تاريخها
                        var expiredBookings = await context.Bookings
                            .Where(b => b.Status == BookingStatus.Confirmed && b.EndDate < now)
                            .ToListAsync(stoppingToken);

                        foreach (var booking in expiredBookings)
                        {
                            booking.Status = BookingStatus.Completed;
                            _logger.LogInformation("Booking {BookingId} set to Completed.", booking.Id);
                        }

                        // 2. إلغاء الحجوزات المعلقة اللي مر عليها يوم من غير موافقة
                        var pendingTimeout = await context.Bookings
                            .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt.AddDays(1) < now)
                            .ToListAsync(stoppingToken);

                        foreach (var booking in pendingTimeout)
                        {
                            booking.Status = BookingStatus.Cancelled;
                            _logger.LogInformation("Booking {BookingId} cancelled due to timeout.", booking.Id);
                        }

                        if (expiredBookings.Any() || pendingTimeout.Any())
                        {
                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating booking statuses.");
                }

                // يشتغل كل ساعة (ممكن تغيرها لـ FromMinutes(30) لو عايز سرعة أكتر)
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
