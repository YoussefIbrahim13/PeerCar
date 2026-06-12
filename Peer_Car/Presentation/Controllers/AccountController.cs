using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Peer_Car.Application.Interfaces;
using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;

namespace Peer_Car.Presentation.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly SignInManager<User> _signInManager;
        private readonly IFileService _fileService; // Injected FileService

        public AccountController(
            IAccountService accountService,
            UserManager<User> userManager,
            ILogger<AccountController> logger,
            SignInManager<User> signInManager,
            IFileService fileService)
        {
            _accountService = accountService;
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
            _fileService = fileService;
        }

        #region Login & Logout
        [HttpGet("login")]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _accountService.LoginAsync(model);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in.", model.Email);
                return LocalRedirect(returnUrl ?? "/Home/Index");
            }

            if (result.IsNotAllowed)
                ModelState.AddModelError("", "Please confirm your email before logging in.");
            else
                ModelState.AddModelError("", "Invalid login attempt.");

            return View(model);
        }

        [HttpPost("logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Registration & Confirmation
        [HttpGet("register")]
        public IActionResult Register() => View();

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Generate confirmation link after user creation
            var result = await _accountService.RegisterAsync(model, (user, token) =>
                Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme) ?? "");

            if (result.Succeeded) return View("RegistrationConfirmation");

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
        {
            var result = await _accountService.ConfirmEmailAsync(userId, token);
            if (result.Succeeded) return RedirectToAction(nameof(Login));
            return View("Error");
        }

        [HttpGet("confirm-email-change")]
        public async Task<IActionResult> ConfirmEmailChange(Guid userId, string newEmail, string token)
        {
            var result = await _accountService.ConfirmEmailChangeAsync(userId, newEmail, token);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Email updated successfully.";
                return RedirectToAction(nameof(EditProfile));
            }
            return View("Error");
        }
        #endregion

        #region Profile Management
        [HttpGet("edit-profile")]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            var model = await _accountService.GetEditProfileModelAsync(user);
            return View(model);
        }

        [HttpPost("edit-profile")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            // Clean phone number format
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                model.PhoneNumber = model.PhoneNumber.Trim().Replace(" ", "");
            }

            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            // 1. Handle Identity Documents Upload
            if (model.IdFrontFile != null)
                user.NationalIdFrontUrl = await _fileService.SaveDocumentAsync(model.IdFrontFile, "NationalID");

            if (model.IdBackFile != null)
                user.NationalIdBackUrl = await _fileService.SaveDocumentAsync(model.IdBackFile, "NationalID");

            if (model.LicenseFrontFile != null)
                user.LicenseFrontUrl = await _fileService.SaveDocumentAsync(model.LicenseFrontFile, "License");

            if (model.LicenseBackFile != null)
                user.LicenseBackUrl = await _fileService.SaveDocumentAsync(model.LicenseBackFile, "License");

            // 2. Check for Email Change
            string? emailLink = null;
            if (user.Email != model.Email)
            {
                var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
                emailLink = Url.Action("ConfirmEmailChange", "Account",
                    new { userId = user.Id, newEmail = model.Email, token }, Request.Scheme);
            }

            // 3. Update Profile Data via Service
            var result = await _accountService.UpdateProfileAsync(user, model, emailLink);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile and documents updated successfully.";
                return RedirectToAction(nameof(EditProfile));
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(model);
        }

        [HttpPost("remove-profile-image")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProfileImage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await _accountService.RemoveProfileImageAsync(user);
                TempData["SuccessMessage"] = "Profile picture removed.";
            }
            return RedirectToAction(nameof(EditProfile));
        }
        #endregion

        #region Password Recovery
        [HttpGet("forgot-password")]
        public IActionResult ForgotPassword() => View();

        [HttpPost("forgot-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("Email", "The email address you entered is not registered.");
                return View(model);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("Email", "Please confirm your email before requesting a password reset.");
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account",
                new { email = user.Email, token }, Request.Scheme);

            var subject = "Reset Your PeerCar Password";
            var body = $"<h3>Hello {user.FirstName},</h3><p>Please reset your password by <a href='{resetLink}'>clicking here</a>.</p>";

            await _accountService.SendEmailGenericAsync(user.Email!, subject, body);

            return View("ForgotPasswordConfirmation");
        }

        [HttpGet("reset-password")]
        public IActionResult ResetPassword(string email, string token)
        {
            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _accountService.ResetPasswordAsync(model);

            if (result.Succeeded)
            {
                TempData["ToastSuccess"] = "Password updated successfully!";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                if (error.Code == "InvalidToken")
                {
                    ModelState.AddModelError("", "The reset link has expired. Please request a new one.");
                }
                else
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
        #endregion

        #region Account Actions
        [HttpPost("delete-account")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            var result = await _accountService.DeleteAccountAsync(user);
            if (result.Succeeded)
            {
                await _accountService.LogoutAsync();
                return RedirectToAction("Index", "Home");
            }

            return View("Error");
        }

        [HttpPost("make-me-admin")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeMeAdmin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null) await _accountService.MakeAdminAsync(user);
            return RedirectToAction("EditProfile");
        }
        #endregion

        [HttpGet("my-account")]
        [Authorize]
        public async Task<IActionResult> MyAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            var model = await _accountService.GetEditProfileModelAsync(user);
            return View(model);
        }

        [HttpGet("profile/{id:guid}")]
        public async Task<IActionResult> UserProfile(Guid id)
        {
            var currentUserIdString = _userManager.GetUserId(User);
            Guid currentUserId = currentUserIdString != null ? Guid.Parse(currentUserIdString) : Guid.Empty;

            var viewModel = await _accountService.GetUserProfileForViewingAsync(id, currentUserId);

            if (viewModel == null) return NotFound();

            return View("MyAccount", viewModel);
        }

        #region External Login (Google)

        [HttpPost("external-login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = await _accountService.ConfigureExternalAuthenticationProperties(provider, redirectUrl!);
            return new ChallengeResult(provider, properties);
        }

        [HttpGet("external-login-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                ModelState.AddModelError("", $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }

            var info = await _accountService.GetExternalLoginInfoAsync();
            if (info == null) return RedirectToAction(nameof(Login));

            var result = await _accountService.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey);

            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            var (identityResult, user) = await _accountService.CreateUserFromExternalAsync(info);

            if (identityResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            return View(nameof(Login));
        }
        #endregion
    }
}