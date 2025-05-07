using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;

public class CarsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CarsController> _logger;

    public CarsController(ApplicationDbContext context, ILogger<CarsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Cars
    public async Task<IActionResult> Index()
    {
        try
        {
            var cars = await _context.Cars.ToListAsync();
            return View(cars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "حدث خطأ أثناء تحميل السيارات.");
            ModelState.AddModelError(string.Empty, "حدث خطأ أثناء تحميل السيارات.");
            return View(new List<Car>());
        }
    }

    // GET: Cars/Create
    public IActionResult Create()
    {
        // تحويل الـ enum إلى قائمة SelectListItem
        ViewBag.AvailabilityStatuses = new SelectList(Enum.GetValues(typeof(Car.CarAvailabilityStatus))
                                        .Cast<Car.CarAvailabilityStatus>()
                                        .Select(e => new SelectListItem
                                        {
                                            Text = e.ToString(),
                                            Value = e.ToString()
                                        }), "Value", "Text");

        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Create([Bind("Id,Brand,Model,PricePerDay,AvailabilityStatus")] Car car)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // إضافة السيارة إلى قاعدة البيانات
                _context.Add(car);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ أثناء حفظ السيارة.");
                ModelState.AddModelError(string.Empty, "حدث خطأ أثناء حفظ السيارة.");
            }
        }

        // إعادة قائمة الحالات في حالة حدوث خطأ
        // هنا يتم استخدام Enum.GetValues بشكل صحيح
        ViewBag.AvailabilityStatuses = new SelectList(Enum.GetValues(typeof(Car.CarAvailabilityStatus))
                                       .Cast<Car.CarAvailabilityStatus>()
                                       .Select(e => new SelectListItem
                                       {
                                           Text = e.ToString(),
                                           Value = e.ToString()
                                       }), "Value", "Text");

        return View(car);
    }

    // GET: Cars/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var car = await _context.Cars.FindAsync(id);
        if (car == null)
        {
            return NotFound();
        }

        // توفير قائمة الحالات المتاحة عند تعديل السيارة
        ViewBag.AvailabilityStatuses = new SelectList(Enum.GetValues(typeof(Car.CarAvailabilityStatus))
                                        .Cast<Car.CarAvailabilityStatus>()
                                        .Select(e => new SelectListItem
                                        {
                                            Text = e.ToString(),
                                            Value = e.ToString()
                                        }), "Value", "Text");

        return View(car);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Brand,Model,PricePerDay,AvailabilityStatus")] Car car)
    {
        if (id != car.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(car);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ أثناء تحديث السيارة.");
                ModelState.AddModelError(string.Empty, "حدث خطأ أثناء تحديث السيارة.");
            }
        }

        // إعادة قائمة الحالات في حالة حدوث خطأ
        ViewBag.AvailabilityStatuses = new SelectList(Enum.GetValues(typeof(Car.CarAvailabilityStatus))
                                          .Cast<Car.CarAvailabilityStatus>()
                                          .Select(e => new SelectListItem
                                          {
                                              Text = e.ToString(), // عرض اسم الـ enum كـ Text
                                              Value = e.ToString() // القيمة الفعلية (النص)
                                          }), "Value", "Text");
        return View(car);
    }
}
