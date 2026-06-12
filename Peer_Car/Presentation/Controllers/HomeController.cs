using Microsoft.AspNetCore.Mvc;
using Peer_Car.Application.ViewModels;
using System.Diagnostics;

namespace Peer_Car.Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("~/Presentation/Views/Home/Index.cshtml");
        }

        public IActionResult Privacy()
        {
            return View("~/Presentation/Views/Home/Privacy.cshtml");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(model);
        }
    }
}
