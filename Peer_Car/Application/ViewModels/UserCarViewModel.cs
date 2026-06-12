namespace Peer_Car.Application.ViewModels
{
    public class UserCarViewModel
    {
        // بدل List<Car>، هنستخدم List من ViewModel مبسط للعربية
        public List<CarViewModel> ApprovedCars { get; set; } = new List<CarViewModel>();

        // دول تمام لأنهم بياخدوا من ViewModel تانية فعلاً
        public List<CarSubmissionViewModel> PendingSubmissions { get; set; } = new List<CarSubmissionViewModel>();

        public List<CarSubmissionViewModel> RejectedSubmissions { get; set; } = new List<CarSubmissionViewModel>();
    }
}
