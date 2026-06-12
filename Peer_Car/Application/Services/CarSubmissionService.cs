using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peer_Car.Application.Interfaces;
using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;
using Peer_Car.Infrastructure.Data;

namespace Peer_Car.Application.Services
{
  
    public class CarSubmissionService : ICarSubmissionService
    {
        private readonly ApplicationDbContext _context;

        public CarSubmissionService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<IGrouping<string, CarSubmissionViewModel>>> GetGroupedPendingSubmissionsAsync()
        {
            
            var submissions = await _context.CarSubmissions
                .Include(cs => cs.Car)
                .Include(cs => cs.SubmittedBy)
                .Where(cs => cs.Status == CarSubmissionStatus.Pending)
                .OrderBy(cs => cs.SubmissionDate)
                .ToListAsync(); 

            var viewModels = submissions
                .Select(cs => MapToViewModel(cs))
                .GroupBy(vm => vm.SubmittedByName)
                .OrderBy(g => g.Key);

            return viewModels;
        }
        public async Task<CarSubmissionViewModel?> GetSubmissionDetailsAsync(Guid id)
        {
            var submission = await _context.CarSubmissions
                .Include(cs => cs.Car)
                .Include(cs => cs.SubmittedBy)
                .Include(cs => cs.ApprovedBy)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            return submission == null ? null : MapToViewModel(submission);
        }

        public async Task<IdentityResult> ApproveSubmissionAsync(Guid id, Guid adminId)
        {
            var submission = await _context.CarSubmissions
                .Include(cs => cs.Car)
                .FirstOrDefaultAsync(cs => cs.Id == id && cs.Status == CarSubmissionStatus.Pending);

            if (submission == null) return IdentityResult.Failed(new IdentityError { Description = "Submission not found." });

            // تحديث الطلب
            submission.Status = CarSubmissionStatus.Approved;
            submission.ApprovedById = adminId;
            submission.ApprovalDate = DateTime.UtcNow;

            // تحديث العربية لتظهر للعامة
            submission.Car.SubmissionStatus = CarSubmissionStatus.Approved;
            submission.Car.ApprovalDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RejectSubmissionAsync(Guid id, string reason, Guid adminId)
        {
            var submission = await _context.CarSubmissions
                .Include(cs => cs.Car)
                .FirstOrDefaultAsync(cs => cs.Id == id && cs.Status == CarSubmissionStatus.Pending);

            if (submission == null) return IdentityResult.Failed(new IdentityError { Description = "Submission not found." });

            submission.Status = CarSubmissionStatus.Rejected;
            submission.RejectionReason = reason;
            submission.ApprovedById = adminId;
            submission.ApprovalDate = DateTime.UtcNow;

            submission.Car.SubmissionStatus = CarSubmissionStatus.Rejected;
            submission.Car.RejectionReason = reason;

            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        // Helper للمنع تكرار الكود (Mapping)
        private CarSubmissionViewModel MapToViewModel(CarSubmission cs) => new()
        {
            Id = cs.Id,
            CarId = cs.CarId,
            CarBrand = cs.Car.Brand,
            CarModel = cs.Car.Model,
            CarYear = cs.Car.Year,               // ضفنا السنة
            CarPricePerDay = cs.Car.PricePerDay, // ضفنا السعر
            CarLocation = cs.Car.Location,       // ضفنا المكان
            CarImagePath = cs.Car.ImagePath,
            CarDocumentPath = cs.Car.DocumentPath, // ضفنا رابط المستند (مهم جداً!)
            CarDescription = cs.Car.Brand + " " + cs.Car.Model + " - " + cs.Car.Year, // مثال لوصف تلقائي

            SubmittedByName = cs.SubmittedBy?.FullName ?? "Unknown",
            SubmittedByEmail = cs.SubmittedBy?.Email ?? "",
            SubmissionDate = cs.SubmissionDate,
            Status = cs.Status,
            RejectionReason = cs.RejectionReason,
            AdminNotes = cs.AdminNotes,
            ApprovedByName = cs.ApprovedBy?.FullName,
            ApprovalDate = cs.ApprovalDate
        };
    }
}
