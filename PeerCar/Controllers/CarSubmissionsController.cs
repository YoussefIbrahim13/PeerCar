using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

[Authorize(Roles = "Admin")]
public class CarSubmissionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CarSubmissionsController> _logger;

    public CarSubmissionsController(ApplicationDbContext context, ILogger<CarSubmissionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ADMIN: List all pending car submissions, grouped by user
    public async Task<IActionResult> Index()
    {
        try
        {
            var pendingSubmissions = await _context.CarSubmissions
                .Include(cs => cs.Car)
                .Include(cs => cs.SubmittedBy)
                .Where(cs => cs.Status == CarSubmissionStatus.Pending)
                .OrderBy(cs => cs.SubmissionDate)
                .ToListAsync();

            var viewModels = pendingSubmissions.Select(cs => new CarSubmissionViewModel
            {
                Id = cs.Id,
                Car = cs.Car,
                SubmittedByName = cs.SubmittedBy?.Name ?? "Unknown",
                SubmittedByEmail = cs.SubmittedBy?.Email ?? "",
                SubmissionDate = cs.SubmissionDate,
                Status = cs.Status,
                RejectionReason = cs.RejectionReason,
                AdminNotes = cs.AdminNotes,
                ApprovedByName = cs.ApprovedBy?.Name,
                ApprovalDate = cs.ApprovalDate
            }).ToList();

            // Group by user
            var groupedSubmissions = viewModels
                .GroupBy(vm => vm.SubmittedByName)
                .OrderBy(g => g.Key);

            return View(groupedSubmissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pending submissions");
            ModelState.AddModelError(string.Empty, "An error occurred while loading pending submissions.");
            return View(Enumerable.Empty<IGrouping<string, CarSubmissionViewModel>>());
        }
    }

    // ADMIN: View details of a submission
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var submission = await _context.CarSubmissions
                .Include(cs => cs.Car)
                .Include(cs => cs.SubmittedBy)
                .Include(cs => cs.ApprovedBy)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            if (submission == null)
                return NotFound();

            var viewModel = new CarSubmissionViewModel
            {
                Id = submission.Id,
                Car = submission.Car,
                SubmittedByName = submission.SubmittedBy?.Name ?? "Unknown",
                SubmittedByEmail = submission.SubmittedBy?.Email ?? "",
                SubmissionDate = submission.SubmissionDate,
                Status = submission.Status,
                RejectionReason = submission.RejectionReason,
                AdminNotes = submission.AdminNotes,
                ApprovedByName = submission.ApprovedBy?.Name,
                ApprovalDate = submission.ApprovalDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading submission details");
            ModelState.AddModelError(string.Empty, "An error occurred while loading submission details.");
            return RedirectToAction(nameof(Index));
        }
    }

    // ADMIN: Approve a car submission
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            var submission = await _context.CarSubmissions
                .Include(cs => cs.Car)
                .FirstOrDefaultAsync(cs => cs.Id == id && cs.Status == CarSubmissionStatus.Pending);

            if (submission == null)
            {
                TempData["ErrorMessage"] = "Submission not found or already processed.";
                return RedirectToAction(nameof(Index));
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Update submission
            submission.Status = CarSubmissionStatus.Approved;
            submission.ApprovedById = adminId;
            submission.ApprovalDate = DateTime.Now;

            // Update car
            submission.Car.SubmissionStatus = CarSubmissionStatus.Approved;
            submission.Car.ApprovalDate = DateTime.Now;

            _context.Update(submission);
            _context.Update(submission.Car);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Car '{submission.Car.Brand} {submission.Car.Model}' has been approved successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving submission {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while approving the submission.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ADMIN: Reject a car submission (with reason)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Rejection reason is required.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            var submission = await _context.CarSubmissions
                .Include(cs => cs.Car)
                .FirstOrDefaultAsync(cs => cs.Id == id && cs.Status == CarSubmissionStatus.Pending);

            if (submission == null)
            {
                TempData["ErrorMessage"] = "Submission not found or already processed.";
                return RedirectToAction(nameof(Index));
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Update submission
            submission.Status = CarSubmissionStatus.Rejected;
            submission.RejectionReason = reason;
            submission.ApprovedById = adminId;
            submission.ApprovalDate = DateTime.Now;

            // Update car
            submission.Car.SubmissionStatus = CarSubmissionStatus.Rejected;
            submission.Car.RejectionReason = reason;

            _context.Update(submission);
            _context.Update(submission.Car);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Car '{submission.Car.Brand} {submission.Car.Model}' has been rejected.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting submission {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while rejecting the submission.";
            return RedirectToAction(nameof(Index));
        }
    }
} 