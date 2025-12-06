using System.ComponentModel.DataAnnotations;

namespace CarRentalMVC.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
