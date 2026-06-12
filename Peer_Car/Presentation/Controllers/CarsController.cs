using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Peer_Car.Application.Interfaces;
using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;
using System.Security.Claims;

namespace Peer_Car.Presentation.Controllers
{
    [Route("cars")]
    public class CarsController : Controller
    {
        private readonly ICarService _carService;
        private readonly ILogger<CarsController> _logger;

        public CarsController(ICarService carService, ILogger<CarsController> logger)
        {
            _carService = carService;
            _logger = logger;
        }

        #region Public Operations (Browsing)

        [HttpGet("")]
        public async Task<IActionResult> Index(string? brand, string? priceRange)
        {
            var carsEntity = await _carService.GetApprovedCarsAsync(brand, priceRange);

            var viewModel = carsEntity.Select(c => new CarViewModel
            {
                Id = c.Id,
                Brand = c.Brand,
                Model = c.Model,
                Year = c.Year,
                PricePerDay = c.PricePerDay,
                Location = c.Location,
                ImagePath = c.ImagePath,
                OwnerName = c.Owner?.FullName ?? "Unknown"
            }).ToList();

            ViewBag.Brands = await _carService.GetDistinctBrandsAsync();
            ViewBag.SelectedBrand = brand;
            ViewBag.SelectedPriceRange = priceRange;

            return View(viewModel);
        }

        [HttpGet("details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var car = await _carService.GetCarByIdAsync(id);
            if (car == null) return NotFound();

            return View(car);
        }

        #endregion

        #region User Operations (My Cars & Submission)

        [Authorize]
        [HttpGet("my-cars")]
        public async Task<IActionResult> MyCars()
        {
            var userId = GetUserId();
            var viewModel = await _carService.GetUserCarsAndSubmissionsAsync(userId);
            return View(viewModel);
        }

        [Authorize]
        [HttpGet("submit")]
        public IActionResult Submit() => View();

        [Authorize]
        [HttpPost("submit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Car car, IFormFile carImage, IFormFile carDocument)
        {
            if (carImage == null || carDocument == null)
            {
                ModelState.AddModelError("", "You must upload both the car image and the legal document.");
                return View(car);
            }

            if (!ModelState.IsValid) return View(car);

            await _carService.SubmitCarByUserAsync(car, carImage, carDocument, GetUserId());
            TempData["SuccessMessage"] = "Car submitted successfully for review!";
            return RedirectToAction(nameof(MyCars));
        }

        #endregion

        #region Management Operations (Owner & Admin Edit)

        [Authorize]
        [HttpGet("edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var car = await _carService.GetCarByIdAsync(id);
            if (car == null) return NotFound();

            var currentUserId = GetUserId();
            bool isOwner = car.OwnerId == currentUserId;

            if (!isOwner)
            {
                return Forbid();
            }

            return View(car);
        }

        [Authorize]
        [HttpPost("edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Car car, IFormFile? carImage)
        {
            var existingCar = await _carService.GetCarByIdAsync(id);
            if (existingCar == null) return NotFound();

            if (existingCar.OwnerId != GetUserId()) return Forbid();

            if (!ModelState.IsValid) return View(car);

            await _carService.UpdateCarAsync(id, car, carImage);

            TempData["SuccessMessage"] = "Car updated successfully!";
            return RedirectToAction(nameof(MyCars));
        }

        [Authorize]
        [HttpGet("edit-rejected/{id:guid}")]
        public async Task<IActionResult> EditRejected(Guid id)
        {
            var car = await _carService.GetCarByIdAsync(id);
            if (car == null || car.OwnerId != GetUserId() || car.SubmissionStatus != CarSubmissionStatus.Rejected)
                return NotFound();

            return View(car);
        }

        [Authorize]
        [HttpPost("edit-rejected/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRejected(Guid id, Car car, IFormFile? carImage, IFormFile? carDocument)
        {
            if (!ModelState.IsValid) return View(car);

            await _carService.ResubmitRejectedCarAsync(id, car, carImage, carDocument, GetUserId());
            TempData["SuccessMessage"] = "Car resubmitted successfully!";
            return RedirectToAction(nameof(MyCars));
        }

        #endregion

        #region Admin Only Operations

       
        [Authorize(Roles = "Admin")]
        [HttpPost("delete/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _carService.DeleteCarAsync(id);
            TempData["SuccessMessage"] = "Car deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        private Guid GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim != null ? Guid.Parse(claim) : Guid.Empty;
        }
    }
}