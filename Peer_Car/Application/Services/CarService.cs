using Microsoft.EntityFrameworkCore;
using Peer_Car.Application.Interfaces;
using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;
using Peer_Car.Infrastructure.Data;

namespace Peer_Car.Application.Services
{
    public class CarService : ICarService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CarService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        #region Read Operations

        public async Task<IEnumerable<Car>> GetApprovedCarsAsync(string? brand, string? priceRange)
        {
            var query = _context.Cars
                .Include(c => c.Owner)
                .Where(c => c.SubmissionStatus == CarSubmissionStatus.Approved)
                .AsQueryable();

            if (!string.IsNullOrEmpty(brand))
                query = query.Where(c => c.Brand == brand);
            if (!string.IsNullOrEmpty(priceRange))
            {
                if (priceRange.EndsWith("+"))
                {
                    if (decimal.TryParse(priceRange.Replace("+", ""), out decimal minPrice))
                    {
                        query = query.Where(c => c.PricePerDay >= minPrice);
                    }
                }
                else if (priceRange.Contains("-"))
                {
                    var parts = priceRange.Split('-');
                    if (parts.Length == 2 &&
                        decimal.TryParse(parts[0], out decimal min) &&
                        decimal.TryParse(parts[1], out decimal max))
                    {
                        query = query.Where(c => c.PricePerDay >= min && c.PricePerDay <= max);
                    }
                }
            }

            return await query.ToListAsync();
        }

        public async Task<Car?> GetCarByIdAsync(Guid id) =>
            await _context.Cars.Include(c => c.Owner).FirstOrDefaultAsync(c => c.Id == id);

        public async Task<IEnumerable<string>> GetDistinctBrandsAsync() =>
            await _context.Cars.Where(c => c.SubmissionStatus == CarSubmissionStatus.Approved)
                              .Select(c => c.Brand).Distinct().ToListAsync();

        public async Task<UserCarViewModel> GetUserCarsAndSubmissionsAsync(Guid userId)
        {
            // 1. هات الداتا من الداتابيز (Entities)
            var cars = await _context.Cars
                .Where(c => c.OwnerId == userId)
                .ToListAsync();

            var subs = await _context.CarSubmissions
                .Include(cs => cs.Car)
                .Where(cs => cs.SubmittedById == userId)
                .ToListAsync();

            // 2. حول الداتا لـ ViewModels وأنت بترجعها
            return new UserCarViewModel
            {
                // تحويل لستة العربيات المقبولة
                ApprovedCars = cars
                    .Where(c => c.SubmissionStatus == CarSubmissionStatus.Approved)
                    .Select(c => new CarViewModel // تحويل Entity لـ ViewModel
                    {
                        Id = c.Id,
                        Brand = c.Brand,
                        Model = c.Model,
                        Year = c.Year,
                        PricePerDay = c.PricePerDay,
                        ImagePath = c.ImagePath,
                        Location = c.Location,
                        AvailabilityStatus = c.AvailabilityStatus
                    }).ToList(),

                // تحويل لستة الطلبات المعلقة
                PendingSubmissions = subs
                    .Where(s => s.Status == CarSubmissionStatus.Pending)
                    .Select(s => new CarSubmissionViewModel
                    {
                        Id = s.Id,
                        CarBrand = s.Car.Brand,
                        CarModel = s.Car.Model,
                        SubmissionDate = s.SubmissionDate,
                        Status = s.Status,
                        CarImagePath = s.Car.ImagePath
                    }).ToList(),

                // تحويل لستة الطلبات المرفوضة
                RejectedSubmissions = subs
                    .Where(s => s.Status == CarSubmissionStatus.Rejected)
                    .Select(s => new CarSubmissionViewModel
                    {
                        Id = s.Id,
                        CarBrand = s.Car.Brand,
                        CarModel = s.Car.Model,
                        SubmissionDate = s.SubmissionDate,
                        Status = s.Status,
                        RejectionReason = s.RejectionReason,
                        CarImagePath = s.Car.ImagePath
                    }).ToList()
            };
        }
        #endregion

        #region Write Operations

        public async Task SubmitCarByUserAsync(Car car, IFormFile image, IFormFile document, Guid userId)
        {
            car.ImagePath = await UploadFile(image, "images/cars");
            car.DocumentPath = await UploadFile(document, "documents/cars");
            car.OwnerId = userId;
            car.SubmissionStatus = CarSubmissionStatus.Pending;
            car.SubmissionDate = DateTime.UtcNow;
            car.AvailabilityStatus = CarAvailabilityStatus.Available;

            _context.Cars.Add(car);

            _context.CarSubmissions.Add(new CarSubmission
            {
                CarId = car.Id,
                SubmittedById = userId,
                SubmissionDate = DateTime.UtcNow,
                Status = CarSubmissionStatus.Pending
            });

            await _context.SaveChangesAsync();
        }

        //public async Task CreateCarByAdminAsync(Car car, IFormFile? image, Guid adminId)
        //{
        //    if (image != null) car.ImagePath = await UploadFile(image, "images/cars");

        //    car.OwnerId = adminId;
        //    car.SubmissionStatus = CarSubmissionStatus.Approved;
        //    car.ApprovalDate = DateTime.UtcNow;
        //    car.SubmissionDate = DateTime.UtcNow;

        //    _context.Cars.Add(car);
        //    await _context.SaveChangesAsync();
        //}

        public async Task UpdateCarAsync(Guid id, Car car, IFormFile? image)
        {
            // 1. البحث عن العربية في الداتابيز
            var carInDb = await _context.Cars.FindAsync(id);
            if (carInDb == null) return;

            // 2. تحديث الصورة لو اليوزر رفع صورة جديدة
            if (image != null)
            {
                // مسح الصورة القديمة من السيرفر عشان ما نزحمش المساحة
                if (!string.IsNullOrEmpty(carInDb.ImagePath))
                {
                    DeleteFile(carInDb.ImagePath);
                }

                // رفع الصورة الجديدة وحفظ المسار
                carInDb.ImagePath = await UploadFile(image, "images/cars");
            }

            // 3. تحديث البيانات الأساسية
            carInDb.Brand = car.Brand;
            carInDb.Model = car.Model;
            carInDb.Year = car.Year;
            carInDb.Location = car.Location;
            carInDb.PricePerDay = car.PricePerDay;

            // ملاحظة: حالة التوافر (AvailabilityStatus) غالباً المالك مش بيغيرها يدوي 
            // هي بتتغير أوتوماتيك مع الحجوزات، بس لو عايز تسيبها للتعديل اليدوي سيب السطر ده:
            carInDb.AvailabilityStatus = car.AvailabilityStatus;

            // 4. حفظ التغييرات
            _context.Cars.Update(carInDb);
            await _context.SaveChangesAsync();
        }

        public async Task ResubmitRejectedCarAsync(Guid carId, Car updatedCar, IFormFile? image, IFormFile? document, Guid userId)
        {
            var carInDb = await _context.Cars.FirstOrDefaultAsync(c => c.Id == carId && c.OwnerId == userId);
            if (carInDb == null) return;

            // تحديث الملفات لو اترفع جديد
            if (image != null)
            {
                DeleteFile(carInDb.ImagePath);
                carInDb.ImagePath = await UploadFile(image, "images/cars");
            }
            if (document != null)
            {
                DeleteFile(carInDb.DocumentPath);
                carInDb.DocumentPath = await UploadFile(document, "documents/cars");
            }

            // تحديث البيانات الأساسية
            carInDb.Brand = updatedCar.Brand;
            carInDb.Model = updatedCar.Model;
            carInDb.Year = updatedCar.Year;
            carInDb.Location = updatedCar.Location;
            carInDb.PricePerDay = updatedCar.PricePerDay;

            // إعادة التقديم للمراجعة
            carInDb.SubmissionStatus = CarSubmissionStatus.Pending;
            carInDb.SubmissionDate = DateTime.UtcNow;
            carInDb.RejectionReason = null; // تصفير سبب الرفض القديم

            _context.Update(carInDb);

            // إضافة سجل تقديم جديد للـ History
            _context.CarSubmissions.Add(new CarSubmission
            {
                CarId = carInDb.Id,
                SubmittedById = userId,
                SubmissionDate = DateTime.UtcNow,
                Status = CarSubmissionStatus.Pending
            });

            await _context.SaveChangesAsync();
        }

        public async Task DeleteCarAsync(Guid id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car != null)
            {
                
                car.IsDeleted = true;

                
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Helpers

        private async Task<string> UploadFile(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{folder}/{fileName}";
        }

        private void DeleteFile(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                try { File.Delete(fullPath); }
                catch { /* Log error or ignore */ }
            }
        }

        #endregion


        public async Task<IEnumerable<CarViewModel>> GetRecentCarsAsync(int count)
        {
            
            var cars = await _context.Cars
                .Include(c => c.Owner)
                .Include(c => c.Reviews)
                .OrderByDescending(c => c.CreatedAt) 
                .Take(count)
                .ToListAsync();

            return cars.Select(c => new CarViewModel
            {
                Id = c.Id,
                Brand = c.Brand,
                Model = c.Model,
                Year = c.Year,
                PricePerDay = c.PricePerDay,
                Location = c.Location ?? "Not Specified",
                ImagePath = c.ImagePath ?? "/images/cars/default-car.png",

                OwnerId = c.OwnerId,
                OwnerName = c.Owner?.FullName ?? "Unknown Owner",

                AvailabilityStatus = c.AvailabilityStatus,
                SubmissionStatus = c.SubmissionStatus,

                AverageRating = c.Reviews != null && c.Reviews.Any()
                                ? Math.Round(c.Reviews.Average(r => r.Rating), 1)
                                : 0,

                TotalReviews = c.Reviews?.Count ?? 0
            });
        }



    }
}
