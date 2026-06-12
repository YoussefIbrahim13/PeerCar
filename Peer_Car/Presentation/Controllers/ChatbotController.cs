using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Peer_Car.Presentation.Controllers
{
    [Authorize]
    public class ChatbotController : Controller
    {
        [Route("chatbot")]
        public IActionResult Index()
        {
            return View();
        }
    }
}