using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // تأكد من وجود دي عشان IFormFile

namespace Peer_Car.Application.ViewModels
{
    public class EditProfileViewModel
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Only Gmail addresses are allowed.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^1[0-9]{9}$", ErrorMessage = "Enter a valid Egyptian number (10 digits starting with 1).")]
        public string PhoneNumber { get; set; }

        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }

        public string? ProfileImageUrl { get; set; }
        public IFormFile? ProfileImage { get; set; }

        // --- روابط الصور الحالية (لعرضها) ---
        public string? NationalIdFrontUrl { get; set; }
        public string? NationalIdBackUrl { get; set; }
        public string? LicenseFrontUrl { get; set; }
        public string? LicenseBackUrl { get; set; }

        // --- الخصائص الجديدة لاستقبال الملفات المرفوعة (حل المشكلة) ---
        public IFormFile? IdFrontFile { get; set; }
        public IFormFile? IdBackFile { get; set; }
        public IFormFile? LicenseFrontFile { get; set; }
        public IFormFile? LicenseBackFile { get; set; }

        public bool IsOwnerViewing { get; set; }
    }
}