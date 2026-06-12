using Microsoft.AspNetCore.Mvc;
using Peer_Car.Application.Interfaces;
using Peer_Car.Application.Services;

namespace Peer_Car.Presentation.Controllers
{
    [Route("users")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<ReviewService> logger)
        {
            _userService = userService;
        }

        // GET: /users/profile/{id}
        [HttpGet("profile/{id:guid}")]
        public async Task<IActionResult> Profile(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var model = await _userService.GetUserProfileAsync(id);

            if (model == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", id);
                return NotFound();
            }

            return View(model);
        }
    }
}
