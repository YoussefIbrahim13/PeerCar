using System.ComponentModel.DataAnnotations;

namespace CarRentalMVC.Models
{
    public class UserCarViewModel
    {
        public List<Car> ApprovedCars { get; set; } = new List<Car>();
        public List<CarSubmissionViewModel> PendingSubmissions { get; set; } = new List<CarSubmissionViewModel>();
        public List<CarSubmissionViewModel> RejectedSubmissions { get; set; } = new List<CarSubmissionViewModel>();
    }
} 