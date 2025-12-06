using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CarRentalMVC.Models
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Only Gmail addresses are allowed.")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        [RegularExpression(@"^1[0-9]{9}$", ErrorMessage = "Enter a valid Egyptian number (10 digits starting with 1, e.g., 1001234567).")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        public IFormFile? ProfileImage { get; set; }
    }
}
