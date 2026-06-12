using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peer_Car.Application.Interfaces;
using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;
using Peer_Car.Infrastructure.Data;

namespace Peer_Car.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Read Operations

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid userId)
        {
            return await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.RenterId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingDetailsAsync(Guid id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Car)
                    .ThenInclude(c => c.Owner)
                .Include(b => b.Renter)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return null;

            if (booking.Status == BookingStatus.Confirmed &&
                !booking.IsKeyHandedOver &&
                booking.StartDate.AddDays(1) < DateTime.UtcNow)
            {
                booking.Status = BookingStatus.Cancelled;
                await _context.SaveChangesAsync();
            }

            return booking;
        }

        public async Task<BookingViewModel?> PrepareBookingViewModelAsync(Guid carId, Guid userId)
        {
            var car = await _context.Cars.Include(c => c.Owner).FirstOrDefaultAsync(c => c.Id == carId);
            if (car == null) return null;

            return new BookingViewModel
            {
                CarId = car.Id,
                RenterId = userId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2),
                PricePerDay = car.PricePerDay,
                CarBrand = car.Brand,
                CarModel = car.Model,
                CarLocation = car.Location,
                CarImagePath = car.ImagePath,
                OwnerUserName = car.Owner?.FullName,
                OwnerPhone = car.Owner?.PhoneNumber
            };
        }

        public async Task<bool> IsCarAvailableAsync(Guid carId, DateTime start, DateTime end)
        {
            return !await _context.Bookings.AnyAsync(b =>
                b.CarId == carId &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Completed &&
                ((start >= b.StartDate && start <= b.EndDate) ||
                 (end >= b.StartDate && end <= b.EndDate) ||
                 (start <= b.StartDate && end >= b.EndDate)));
        }

        #endregion

        #region Write Operations

        public async Task<IdentityResult> CreateBookingAsync(BookingViewModel model, string paymentMethod)
        {
            var car = await _context.Cars.FindAsync(model.CarId);
            if (car == null) return IdentityResult.Failed(new IdentityError { Description = "Car not found." });

            if (!await IsCarAvailableAsync(model.CarId, model.StartDate, model.EndDate))
                return IdentityResult.Failed(new IdentityError { Description = "This car is already booked for the selected dates." });


            var days = (model.EndDate - model.StartDate).Days + 1;
            if (days <= 0) days = 1;

            var booking = new Booking
            {
                CarId = model.CarId,
                RenterId = model.RenterId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                TotalPrice = days * car.PricePerDay,
                Status = BookingStatus.Pending
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        // 1. المالك بيأكد إنه سلم العربية للمستأجر
        public async Task<bool> ConfirmPickupAsync(Guid id, Guid ownerId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.Id == id && b.Car.OwnerId == ownerId);

            // التحقق من الشروط
            if (booking == null || booking.Status != BookingStatus.Confirmed || booking.IsKeyHandedOver)
                return false;

            // التعديل هنا: نثبت إن المفاتيح اتسلمت عشان نحمي الحجز من الإلغاء التلقائي
            booking.IsKeyHandedOver = true;
            booking.IsCarReceivedByUser = true; // دي اللي كانت عندك أصلاً
            booking.CarReceivedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ConfirmReturnAsync(Guid id, Guid renterId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.Id == id && b.RenterId == renterId);

            if (booking == null || !booking.IsCarReceivedByUser || booking.IsCarReturnedByUser) return false;

            booking.IsCarReturnedByUser = true;
            booking.CarReturnedDate = DateTime.UtcNow;
            booking.Status = BookingStatus.Completed; // 🚩 الحجز اكتمل

            if (booking.Car != null)
                booking.Car.AvailabilityStatus = CarAvailabilityStatus.Available;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelBookingAsync(Guid bookingId, Guid userId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.RenterId == userId);

            // بنسمح بالإلغاء فقط لو الحجز لسه "Pending"
            if (booking == null || booking.Status != BookingStatus.Pending)
                return false;

            booking.Status = BookingStatus.Cancelled;

            // لو عايز تخلي العربية متاحة تاني فوراً (لو كانت محجوزة)
            var car = await _context.Cars.FindAsync(booking.CarId);
            if (car != null) car.AvailabilityStatus = CarAvailabilityStatus.Available;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task UpdateStatusAsync(Guid id, BookingStatus status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.Status = status;
                await _context.SaveChangesAsync();
            }
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
        public async Task<bool> ApproveBookingAsync(Guid id, Guid ownerId)
        {
            var booking = await _context.Bookings.Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.Id == id && b.Car.OwnerId == ownerId);
            if (booking == null || booking.Status != BookingStatus.Pending) return false;

            booking.Status = BookingStatus.Confirmed;
            await _context.SaveChangesAsync();
            return true;
        }


        #endregion
        public async Task<IEnumerable<Car>> GetOwnerCarsWithBookingsAsync(Guid ownerId)
        {
            return await _context.Cars
                .Include(c => c.Bookings)
                    .ThenInclude(b => b.Renter) // عشان نعرف مين اللي حجز
                .Where(c => c.OwnerId == ownerId)
                .ToListAsync();
        }
        public async Task<bool> RequestReturnAsync(Guid id, Guid renterId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.RenterId == renterId);
            if (booking == null || !booking.IsCarReceivedByUser || booking.IsCarReturnedByUser) return false;

            booking.IsCarReturnedByUser = true;
            booking.CarReturnedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> FinalizeBookingAsync(Guid id, Guid ownerId)
        {
            var booking = await _context.Bookings.Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.Id == id && b.Car.OwnerId == ownerId);
            if (booking == null || !booking.IsCarReturnedByUser) return false;

            booking.Status = BookingStatus.Completed; // 🚩 هنا ينتهي الحجز تماماً
            if (booking.Car != null) booking.Car.AvailabilityStatus = CarAvailabilityStatus.Available;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
