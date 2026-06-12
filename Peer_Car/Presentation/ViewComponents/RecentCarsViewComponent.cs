using Microsoft.AspNetCore.Mvc;
using Peer_Car.Application.Interfaces;

namespace Peer_Car.Presentation.ViewComponents
{
  
    public class RecentCarsViewComponent : ViewComponent
    {
        private readonly ICarService _carService;

        public RecentCarsViewComponent(ICarService carService)
        {
            _carService = carService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cars = await _carService.GetRecentCarsAsync(6); // مثال
            return View(cars);
        }
    }
}
