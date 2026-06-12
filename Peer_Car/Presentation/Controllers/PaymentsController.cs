using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Peer_Car.Application.Interfaces;
using Peer_Car.Domain.Entities;
using System.Security.Claims;

namespace Peer_Car.Presentation.Controllers
{
    [Authorize]
    [Route("payments")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("my-bookings")]
        public async Task<IActionResult> MyBookings()
        {
            var userId = GetUserId();
            var bookings = await _paymentService.GetUserBookingsForPaymentAsync(userId);
            return View(bookings);
        }

        [HttpGet("create/{bookingId:guid}")]
        public async Task<IActionResult> Create(Guid bookingId)
        {
            var userId = GetUserId();
            var payment = await _paymentService.PreparePaymentAsync(bookingId, userId);

            if (payment == null) return NotFound();

            return View(payment);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (!ModelState.IsValid) return View(payment);

            var userId = GetUserId();
            var result = await _paymentService.ProcessPaymentAsync(payment, userId);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Payment successful! Your booking is confirmed.";
                return RedirectToAction(nameof(MyBookings));
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(payment);
        }

        // Helper لدعم الـ Clean Code
        private Guid GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim != null ? Guid.Parse(claim) : Guid.Empty;
        }
    }
}
