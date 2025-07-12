using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CarRentalMVC.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Only Gmail addresses are allowed.")]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public required string ConfirmPassword { get; set; }

        [Required]
        [RegularExpression(@"^1[0-9]{9}$", ErrorMessage = "Enter a valid Egyptian number (10 digits starting with 1, e.g., 1001234567).")]
        public required string PhoneNumber { get; set; }
    }
}
