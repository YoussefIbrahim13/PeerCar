using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IO;

public class CarsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CarsController> _logger;

    public CarsController(ApplicationDbContext context, ILogger<CarsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private SelectList GetAvailabilityStatuses()
    {
        return new SelectList(Enum.GetValues(typeof(Car.CarAvailabilityStatus))
            .Cast<Car.CarAvailabilityStatus>()
            .Select(e => new SelectListItem
            {
                Text = e.ToString(),
                Value = e.ToString()
            }), "Value", "Text");
    }

    private async Task SetCarAvailabilityStatus(int carId, Car.CarAvailabilityStatus status)
    {
        var car = await _context.Cars.FindAsync(carId);
        if (car != null)
        {
            car.AvailabilityStatus = status;
            _context.Update(car);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SetCarAvailableIfNoPendingBookings(int carId)
    {
        var car = await _context.Cars.FindAsync(carId);
        if (car != null)
        {
            bool hasOtherBookings = await _context.Bookings
                .AnyAsync(b => b.CarId == car.Id && b.Status == BookingStatus.Pending);
            if (!hasOtherBookings)
            {
                car.AvailabilityStatus = Car.CarAvailabilityStatus.Available;
                _context.Update(car);
                await _context.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Displays the list of cars (only approved cars for public view).
    /// </summary>
    // GET: Cars
    public async Task<IActionResult> Index(string brand, string priceRange)
    {
        try
        {
            var carsQuery = _context.Cars.Where(c => c.SubmissionStatus == CarSubmissionStatus.Approved);
            if (!string.IsNullOrEmpty(brand))
            {
                carsQuery = carsQuery.Where(c => c.Brand == brand);
            }
            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "0-50":
                        carsQuery = carsQuery.Where(c => c.PricePerDay >= 0 && c.PricePerDay <= 50);
                        break;
                    case "50-100":
                        carsQuery = carsQuery.Where(c => c.PricePerDay > 50 && c.PricePerDay <= 100);
                        break;
                    case "100-200":
                        carsQuery = carsQuery.Where(c => c.PricePerDay > 100 && c.PricePerDay <= 200);
                        break;
                    case "200+":
                        carsQuery = carsQuery.Where(c => c.PricePerDay > 200);
                        break;
                }
            }
            var cars = await carsQuery.ToListAsync();
            // For dynamic brand dropdown
            ViewBag.Brands = await _context.Cars.Where(c => c.SubmissionStatus == CarSubmissionStatus.Approved).Select(c => c.Brand).Distinct().ToListAsync();
            ViewBag.SelectedBrand = brand;
            ViewBag.SelectedPriceRange = priceRange;
            return View(cars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "حدث خطأ أثناء تحميل السيارات.");
            ModelState.AddModelError(string.Empty, "حدث خطأ أثناء تحميل السيارات.");
            return View(new List<Car>());
        }
    }

    /// <summary>
    /// Displays the details of a specific car.
    /// </summary>
    // GET: Cars/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();
        var car = await _context.Cars
            .Include(c => c.Owner)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (car == null)
            return NotFound();
        return View(car);
    }

    /// <summary>
    /// Shows the car creation form.
    /// </summary>
    // GET: Cars/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        ViewBag.AvailabilityStatuses = GetAvailabilityStatuses();
        return View();
    }

    /// <summary>
    /// Handles the car creation form submission.
    /// </summary>
    // POST: Cars/Create
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([Bind("Brand,Model,Year,Location,PricePerDay,AvailabilityStatus")] Car car)
    {
        var files = Request.Form.Files;
        if (files != null && files.Count > 0)
        {
            var image = files["CarImage"];
            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine("wwwroot", "images", "cars");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                car.ImagePath = "/images/cars/" + uniqueFileName;
            }
        }
        if (ModelState.IsValid)
        {
            car.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
            car.SubmissionStatus = CarSubmissionStatus.Approved; // Admin cars are auto-approved
            car.SubmissionDate = DateTime.Now;
            car.ApprovalDate = DateTime.Now;
            _context.Add(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.AvailabilityStatuses = GetAvailabilityStatuses();
        return View(car);
    }

    /// <summary>
    /// Shows the car editing form.
    /// </summary>
    // GET: Cars/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var car = await _context.Cars.FindAsync(id);
        if (car == null) return NotFound();
        ViewBag.AvailabilityStatuses = GetAvailabilityStatuses();
        return View(car);
    }

    /// <summary>
    /// Handles the car editing form submission.
    /// </summary>
    // POST: Cars/Edit/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Brand,Model,Year,Location,PricePerDay,AvailabilityStatus")] Car car)
    {
        var carInDb = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);
        if (carInDb == null) return NotFound();
        var files = Request.Form.Files;
        if (files != null && files.Count > 0)
        {
            var image = files["CarImage"];
            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine("wwwroot", "images", "cars");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                carInDb.ImagePath = "/images/cars/" + uniqueFileName;
            }
        }
        if (ModelState.IsValid)
        {
            carInDb.Brand = car.Brand;
            carInDb.Model = car.Model;
            carInDb.Year = car.Year;
            carInDb.Location = car.Location;
            carInDb.PricePerDay = car.PricePerDay;
            carInDb.AvailabilityStatus = car.AvailabilityStatus;
            _context.Update(carInDb);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.AvailabilityStatuses = GetAvailabilityStatuses();
        return View(carInDb);
    }

    // USER: Show car submission form
    [Authorize]
    public IActionResult Submit()
    {
        return View();
    }

    // USER: Handle car submission POST
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit([Bind("Brand,Model,Year,Location,PricePerDay")] Car car)
    {
        if (!ModelState.IsValid)
        {
            return View(car);
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Handle file uploads
            var files = Request.Form.Files;
            if (files != null && files.Count > 0)
            {
                // Handle car image
                var image = files["CarImage"];
                if (image != null && image.Length > 0)
                {
                    // Validate image
                    if (image.Length > 5 * 1024 * 1024) // 5MB limit
                    {
                        ModelState.AddModelError("CarImage", "Image file size must be less than 5MB.");
                        return View(car);
                    }

                    var allowedImageTypes = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var imageExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
                    if (!allowedImageTypes.Contains(imageExtension))
                    {
                        ModelState.AddModelError("CarImage", "Only JPG, PNG, GIF, and WebP images are allowed.");
                        return View(car);
                    }

                    var uploadsFolder = Path.Combine("wwwroot", "images", "cars");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + imageExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    car.ImagePath = "/images/cars/" + uniqueFileName;
                }
                else
                {
                    ModelState.AddModelError("CarImage", "Car image is required.");
                    return View(car);
                }

                // Handle document
                var document = files["CarDocument"];
                if (document != null && document.Length > 0)
                {
                    // Validate document
                    if (document.Length > 10 * 1024 * 1024) // 10MB limit
                    {
                        ModelState.AddModelError("CarDocument", "Document file size must be less than 10MB.");
                        return View(car);
                    }

                    var allowedDocTypes = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
                    var docExtension = Path.GetExtension(document.FileName).ToLowerInvariant();
                    if (!allowedDocTypes.Contains(docExtension))
                    {
                        ModelState.AddModelError("CarDocument", "Only PDF, DOC, DOCX, JPG, and PNG documents are allowed.");
                        return View(car);
                    }

                    var documentsFolder = Path.Combine("wwwroot", "documents", "cars");
                    if (!Directory.Exists(documentsFolder))
                        Directory.CreateDirectory(documentsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + docExtension;
                    var filePath = Path.Combine(documentsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await document.CopyToAsync(stream);
                    }
                    car.DocumentPath = "/documents/cars/" + uniqueFileName;
                }
                else
                {
                    ModelState.AddModelError("CarDocument", "Official document is required.");
                    return View(car);
                }
            }
            else
            {
                ModelState.AddModelError("", "Both car image and official document are required.");
                return View(car);
            }

            // Set car properties
            car.OwnerId = userId;
            if (User.IsInRole("Admin"))
            {
                car.SubmissionStatus = CarSubmissionStatus.Approved;
                car.ApprovalDate = DateTime.Now;
            }
            else
            {
                car.SubmissionStatus = CarSubmissionStatus.Pending;
            }
            car.SubmissionDate = DateTime.Now;
            car.AvailabilityStatus = Car.CarAvailabilityStatus.Available;

            // Save car
            _context.Add(car);
            await _context.SaveChangesAsync();

            // Create car submission record
            var submission = new CarSubmission
            {
                CarId = car.Id,
                SubmittedById = userId,
                SubmissionDate = DateTime.Now,
                Status = CarSubmissionStatus.Pending
            };
            _context.CarSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Car submitted successfully! It will be reviewed by an admin.";
            return RedirectToAction(nameof(MyCars));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting car");
            ModelState.AddModelError(string.Empty, "An error occurred while submitting the car.");
            return View(car);
        }
    }

    // USER: View all their cars (approved, pending, rejected)
    [Authorize]
    public async Task<IActionResult> MyCars()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var userCars = await _context.Cars
                .Where(c => c.OwnerId == userId)
                .ToListAsync();

            var submissions = await _context.CarSubmissions
                .Include(cs => cs.Car)
                .Where(cs => cs.SubmittedById == userId)
                .ToListAsync();

            var viewModel = new UserCarViewModel
            {
                ApprovedCars = userCars.Where(c => c.SubmissionStatus == CarSubmissionStatus.Approved).ToList(),
                PendingSubmissions = submissions
                    .Where(cs => cs.Status == CarSubmissionStatus.Pending)
                    .Select(cs => new CarSubmissionViewModel
                    {
                        Id = cs.Id,
                        Car = cs.Car,
                        SubmittedByName = cs.SubmittedBy?.Name ?? "Unknown",
                        SubmittedByEmail = cs.SubmittedBy?.Email ?? "",
                        SubmissionDate = cs.SubmissionDate,
                        Status = cs.Status
                    }).ToList(),
                RejectedSubmissions = submissions
                    .Where(cs => cs.Status == CarSubmissionStatus.Rejected)
                    .Select(cs => new CarSubmissionViewModel
                    {
                        Id = cs.Id,
                        Car = cs.Car,
                        SubmittedByName = cs.SubmittedBy?.Name ?? "Unknown",
                        SubmittedByEmail = cs.SubmittedBy?.Email ?? "",
                        SubmissionDate = cs.SubmissionDate,
                        Status = cs.Status,
                        RejectionReason = cs.RejectionReason
                    }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user cars");
            ModelState.AddModelError(string.Empty, "An error occurred while loading your cars.");
            return View(new UserCarViewModel());
        }
    }

    // USER: Edit and resubmit a rejected car
    [Authorize]
    public async Task<IActionResult> EditRejected(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var car = await _context.Cars
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId && c.SubmissionStatus == CarSubmissionStatus.Rejected);

        if (car == null)
            return NotFound();

        return View(car);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRejected(int id, [Bind("Id,Brand,Model,Year,Location,PricePerDay")] Car car)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var carInDb = await _context.Cars
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId && c.SubmissionStatus == CarSubmissionStatus.Rejected);

        if (carInDb == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            return View(carInDb);
        }

        try
        {
            // Handle file uploads
            var files = Request.Form.Files;
            if (files != null && files.Count > 0)
            {
                // Handle car image
                var image = files["CarImage"];
                if (image != null && image.Length > 0)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "images", "cars");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    carInDb.ImagePath = "/images/cars/" + uniqueFileName;
                }

                // Handle document
                var document = files["CarDocument"];
                if (document != null && document.Length > 0)
                {
                    var documentsFolder = Path.Combine("wwwroot", "documents", "cars");
                    if (!Directory.Exists(documentsFolder))
                        Directory.CreateDirectory(documentsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(document.FileName);
                    var filePath = Path.Combine(documentsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await document.CopyToAsync(stream);
                    }
                    carInDb.DocumentPath = "/documents/cars/" + uniqueFileName;
                }
            }

            // Update car properties
            carInDb.Brand = car.Brand;
            carInDb.Model = car.Model;
            carInDb.Year = car.Year;
            carInDb.Location = car.Location;
            carInDb.PricePerDay = car.PricePerDay;
            carInDb.SubmissionStatus = CarSubmissionStatus.Pending;
            carInDb.SubmissionDate = DateTime.Now;
            carInDb.RejectionReason = null; // Clear rejection reason

            _context.Update(carInDb);

            // Create new submission record
            var submission = new CarSubmission
            {
                CarId = carInDb.Id,
                SubmittedById = userId,
                SubmissionDate = DateTime.Now,
                Status = CarSubmissionStatus.Pending
            };
            _context.CarSubmissions.Add(submission);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Car updated and resubmitted successfully!";
            return RedirectToAction(nameof(MyCars));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resubmitting car");
            ModelState.AddModelError(string.Empty, "An error occurred while resubmitting the car.");
            return View(carInDb);
        }
    }

    /// <summary>
    /// Creates a new booking for a car by the current user (Renter).
    /// </summary>
    /// <param name="booking">The booking details.</param>
    /// <returns>Redirects to MyBookings on success, or returns the view with errors.</returns>
    // POST: Bookings/Create
    [HttpPost]
    [Authorize(Roles = "Renter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBooking(Booking booking)
    {
        if (!ModelState.IsValid)
            return View(booking);

        try
        {
            await CreateBookingInternalAsync(booking);
            return RedirectToAction(nameof(MyBookings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking.");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the booking.");
            return View(booking);
        }
    }

    /// <summary>
    /// Internal method to handle booking creation logic.
    /// </summary>
    /// <param name="booking">The booking to create.</param>
    private async Task CreateBookingInternalAsync(Booking booking)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var car = await _context.Cars.Include(c => c.Owner).FirstOrDefaultAsync(c => c.Id == booking.CarId);
        var renter = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (car == null || renter == null)
            throw new InvalidOperationException("Car or renter not found.");

        booking.RenterId = userId;
        booking.Car = car;
        booking.Renter = renter;
        booking.Status = BookingStatus.Pending;
        _context.Bookings.Add(booking);

        await SetCarAvailabilityStatus(booking.CarId, Car.CarAvailabilityStatus.Reserved);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a booking by its ID. Only accessible by Renter or Admin.
    /// </summary>
    /// <param name="id">The booking ID.</param>
    /// <returns>Redirects to MyBookings on success, or returns NotFound if not found.</returns>
    // POST: Bookings/Delete/5
    [HttpPost]
    [Authorize(Roles = "Renter,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        try
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();
            var carId = booking.CarId;
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            await SetCarAvailableIfNoPendingBookings(carId);
            return RedirectToAction(nameof(MyBookings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting booking with ID {id}.");
            ModelState.AddModelError(string.Empty, "An error occurred while deleting the booking.");
            return RedirectToAction(nameof(MyBookings));
        }
    }

    /// <summary>
    /// Displays the current user's bookings.
    /// </summary>
    /// <returns>The MyBookings view with the user's bookings.</returns>
    [Authorize]
    public async Task<IActionResult> MyBookings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var bookings = await _context.Bookings
            .Include(b => b.Car)
            .Where(b => b.RenterId == userId)
            .ToListAsync();
        return View("~/Views/Bookings/MyBookings.cshtml", bookings);
    }

    /// <summary>
    /// Deletes a car and its image (admin only).
    /// </summary>
    // POST: Cars/Delete/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);
        if (car == null)
        {
            TempData["ErrorMessage"] = "Car not found.";
            return RedirectToAction("Index");
        }
        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Car deleted successfully.";
        return RedirectToAction("Index");
    }
}
