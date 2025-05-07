using System.ComponentModel.DataAnnotations;

namespace CarRentalMVC.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمات المرور غير متطابقة.")]
        public string ConfirmPassword { get; set; }
    }
}
