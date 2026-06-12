using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace CarRentalMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<User> signInManager, 
            UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError(string.Empty, $"You must confirm your email before logging in. <a href=\"/Account/ResendEmailConfirmation?email={user.Email}\" class=\"text-primary text-decoration-underline\">Resend confirmation email</a>");
                        return View(model);
                    }
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);
                        return RedirectToAction("Index", "Home");
                    }
                    // Password incorrect
                    ModelState.AddModelError("Password", "Incorrect password.");
                }
                else
                {
                    // Email not found
                    ModelState.AddModelError("Email", "This email address is not registered.");
                    model.Email = string.Empty;
                }
            }
            return View(model);
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        private string LoadEmailTemplate(string templateName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Views", "Shared", templateName);
            return System.IO.File.ReadAllText(path);
        }

        private string ReplacePlaceholders(string template, System.Collections.Generic.Dictionary<string, string> values)
        {
            foreach (var kvp in values)
            {
                template = template.Replace("{{" + kvp.Key + "}}", kvp.Value ?? "");
            }
            return template;
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate email
                    var emailUser = await _userManager.FindByEmailAsync(model.Email);
                    if (emailUser != null)
                    {
                        ModelState.AddModelError("Email", "This email is already registered.");
                    }
                    // Check for duplicate phone number
                    var phone = model.PhoneNumber ?? string.Empty;
                    if (!string.IsNullOrEmpty(phone))
                    {
                        if (phone.StartsWith("+20")) phone = phone.Substring(3);
                        if (phone.StartsWith("0")) phone = phone.Substring(1);
                        phone = "+20" + phone;
                        var phoneUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
                        if (phoneUser != null)
                        {
                            ModelState.AddModelError("PhoneNumber", "This phone number is already in use.");
                        }
                    }
                    if (!ModelState.IsValid)
                    {
                        return View(model);
                    }
                    var user = new User
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Name = $"{model.FirstName} {model.LastName}",
                        PhoneNumber = phone,
                        Role = UserRole.User,
                        EmailConfirmed = false
                    };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");
                        await _userManager.AddToRoleAsync(user, "Renter");
                        // Email confirmation logic with HTML template
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);
                        var template = LoadEmailTemplate("EmailTemplate.html");
                        var htmlBody = ReplacePlaceholders(template, new System.Collections.Generic.Dictionary<string, string>
                        {
                            { "UserName", user.FirstName ?? string.Empty },
                            { "ConfirmationLink", confirmationLink ?? string.Empty }
                        });
                        await _emailSender.SendEmailAsync(user.Email, "Confirm your email", htmlBody);
                        return View("RegistrationConfirmation");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during user registration");
                    ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                }
            }
            return View(model);
        }

        // GET: Account/ConfirmEmail
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return RedirectToAction("UploadProfilePicture", "Account");
            return View("Error");
        }

        // GET: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");
            var model = new EditProfileViewModel
            {
                FullName = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber != null && user.PhoneNumber.StartsWith("+20") ? user.PhoneNumber.Substring(3) : user.PhoneNumber ?? string.Empty,
                ProfileImage = null
            };
            ViewBag.ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImageUrl) ? null : System.IO.Path.GetFileName(user.ProfileImageUrl);
            return View(model);
        }

        // POST: Account/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            // Only validate phone if it was changed
            var submittedPhone = model.PhoneNumber ?? string.Empty;
            // Remove any leading zeros or +20 prefix from input
            if (submittedPhone.StartsWith("+20"))
                submittedPhone = submittedPhone.Substring(3);
            if (submittedPhone.StartsWith("0"))
                submittedPhone = submittedPhone.Substring(1);
            var currentPhone = user.PhoneNumber != null && user.PhoneNumber.StartsWith("+20") ? user.PhoneNumber.Substring(3) : user.PhoneNumber ?? string.Empty;
            bool phoneChanged = !string.Equals(submittedPhone, currentPhone);

            if (phoneChanged && !string.IsNullOrEmpty(submittedPhone))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(submittedPhone, "^1[0-9]{9}$"))
                {
                    ModelState.AddModelError("PhoneNumber", "Enter a valid Egyptian mobile number (10 digits, starts with 1, e.g., 1001234567)");
                }
            }
            // If phone is empty and required by your business logic, add error here (currently optional)

            if (!ModelState.IsValid) return View(model);

            // Normalize phone number for registration and profile editing
            string fullPhoneNumber = user.PhoneNumber ?? string.Empty;
            if (phoneChanged && !string.IsNullOrEmpty(submittedPhone))
            {
                fullPhoneNumber = "+20" + submittedPhone;
            }

            // Update email if changed
            bool emailChanged = user.Email != model.Email;
            if (emailChanged)
            {
                // Store new email in PendingEmail, do not update Email yet
                user.PendingEmail = model.Email;
                await _userManager.UpdateAsync(user);
                // Generate token for new email
                var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
                var confirmationLink = Url.Action("ConfirmEmailChange", "Account", new { userId = user.Id, newEmail = model.Email, token }, Request.Scheme);
                var template = LoadEmailTemplate("EmailTemplate.html");
                var htmlBody = ReplacePlaceholders(template, new System.Collections.Generic.Dictionary<string, string>
                {
                    { "UserName", user.FirstName ?? string.Empty },
                    { "ConfirmationLink", confirmationLink ?? string.Empty }
                });
                await _emailSender.SendEmailAsync(model.Email, "Confirm your new email", htmlBody);
                return View("EmailChangeConfirmation");
            }
            // Update phone number (with country code)
            if (phoneChanged && !string.IsNullOrEmpty(submittedPhone))
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, fullPhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    foreach (var error in setPhoneResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }
                user.PhoneNumber = fullPhoneNumber;
            }
            // Update full name if changed
            if (user.Name != model.FullName)
            {
                user.Name = model.FullName;
                await _userManager.UpdateAsync(user);
            }
            // Handle profile image upload
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var ext = Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ProfileImage", "Only image files (jpg, jpeg, png, gif) are allowed.");
                    ViewBag.ProfileImageUrl = user.ProfileImageUrl;
                    return View(model);
                }
                var fileName = $"{Guid.NewGuid()}{ext}";
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(stream);
                }
                // Delete old image if exists and not default
                if (!string.IsNullOrEmpty(user.ProfileImageUrl) && user.ProfileImageUrl != "default.png")
                {
                    var oldFileName = Path.GetFileName(user.ProfileImageUrl);
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles", oldFileName);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
                // Store only the filename (views build the /images/profiles/ path)
                user.ProfileImageUrl = fileName;
            }
            // Save changes to user
            await _userManager.UpdateAsync(user);
            // Update password if requested
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is required to change password.");
                    return View(model);
                }
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }
            }
            await _signInManager.RefreshSignInAsync(user);
            if (emailChanged)
                TempData["SuccessMessage"] = "We've sent a confirmation email to your new address. Please check your inbox to confirm the change.";
            else
                TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction("EditProfile");
        }

        // GET: Account/ResendEmailConfirmation
        [HttpGet, HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ToastError"] = "Please enter your email address.";
                return View("Login");
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["ToastError"] = "No user found with this email address.";
                return View("Login");
            }
            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData["ToastError"] = "This email is already confirmed. Please log in.";
                return View("Login");
            }
            // Throttling: allow resend only once every 2 minutes
            string throttleKey = $"ResendEmail_{user.Id}";
            if (TempData[throttleKey] is string lastSentStr && DateTime.TryParse(lastSentStr, out var lastSent))
            {
                if ((DateTime.UtcNow - lastSent).TotalSeconds < 120)
                {
                    TempData["ToastError"] = "You can only resend the confirmation email once every 2 minutes. Please wait and try again.";
                    return View("Login");
                }
            }
            TempData[throttleKey] = DateTime.UtcNow.ToString("o");
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);
            if (!string.IsNullOrEmpty(user.Email))
                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", $"Click <a href='{confirmationLink}'>here</a> to confirm your email.");
            TempData["ToastSuccess"] = "A new confirmation email has been sent. Please check your inbox.";
            return View("Login");
        }

        // GET: Account/ConfirmEmailChange
        [HttpGet]
        public async Task<IActionResult> ConfirmEmailChange(string userId, string newEmail, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newEmail) || string.IsNullOrEmpty(token))
            {
                TempData["ToastError"] = "Invalid email confirmation link.";
                return RedirectToAction("Login");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.PendingEmail != newEmail)
            {
                TempData["ToastError"] = "Invalid or expired email confirmation link.";
                return RedirectToAction("Login");
            }
            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
            if (result.Succeeded)
            {
                user.PendingEmail = null;
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                TempData["ToastSuccess"] = "Your email address has been updated and confirmed.";
                return RedirectToAction("EditProfile");
            }
            TempData["ToastError"] = "Failed to confirm your new email address.";
            return RedirectToAction("EditProfile");
        }

        // GET: Account/MyAccount
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> MyAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");
            return View(user);
        }

        // GET: Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "This email is not registered.");
                return View(model);
            }
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                // Do not reveal that the user is not confirmed
                TempData["SuccessMessage"] = "If an account with that email exists, a password reset link has been sent.";
                return RedirectToAction("ForgotPassword");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account", new { email = user.Email, token }, Request.Scheme);
            var template = LoadEmailTemplate("EmailTemplate.html");
            var htmlBody = ReplacePlaceholders(template, new System.Collections.Generic.Dictionary<string, string>
            {
                { "UserName", user.FirstName ?? string.Empty },
                { "ConfirmationLink", resetLink ?? string.Empty }
            });
            await _emailSender.SendEmailAsync(user.Email, "Reset your password", htmlBody);
            TempData["SuccessMessage"] = "If an account with that email exists, a password reset link has been sent.";
            return RedirectToAction("ForgotPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction("Login");
            }
            var model = new ResetPasswordViewModel { Email = email, Token = token };
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "No user found with this email address.");
                return View(model);
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = "Your password has been changed successfully.";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }
            foreach (var error in result.Errors)
            {
                if (error.Code == "InvalidToken")
                {
                    ModelState.AddModelError(string.Empty, "The password reset link is invalid or has expired. Please request a new reset link.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // GET: Account/UploadProfilePicture
        [HttpGet]
        public IActionResult UploadProfilePicture()
        {
            return View();
        }

        // POST: Account/UploadProfilePicture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
            {
                ModelState.AddModelError("profileImage", "Please select an image file.");
                return View();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"user_{user.Id}_{DateTime.Now.Ticks}{Path.GetExtension(profileImage.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }
            // Store only the filename; views will reference ~/images/profiles/{ProfileImageUrl}
            user.ProfileImageUrl = fileName;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Details", "Account", new { id = user.Id });
        }

        // GET: Account/RequestDeleteAccount
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult RequestDeleteAccount()
        {
            return View();
        }

        // POST: Account/RequestDeleteAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> SubmitDeleteAccountRequest()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var token = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "DeleteAccount");
            var callbackUrl = Url.Action("ConfirmDeleteAccount", "Account", new { userId = user.Id, token }, Request.Scheme);

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Confirm Account Deletion",
                $"Click <a href='{callbackUrl}'>here</a> to confirm deleting your account."
            );

            TempData["SuccessMessage"] = "A confirmation link has been sent to your email.";
            return RedirectToAction("MyAccount");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmDeleteAccount(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var isValid = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "DeleteAccount", token);
            if (!isValid)
            {
                TempData["ErrorMessage"] = "Invalid or expired deletion link.";
                return RedirectToAction("MyAccount");
            }

            // حذف الحجوزات والمدفوعات
            var bookings = await _context.Bookings.Where(b => b.RenterId == user.Id).ToListAsync();
            _context.Bookings.RemoveRange(bookings);

            // حذف المدفوعات المرتبطة بحجوزات المستخدم
            var userBookingIds = await _context.Bookings
                .Where(b => b.RenterId == user.Id)
                .Select(b => b.Id)
                .ToListAsync();

            var payments = await _context.Payments
                .Where(p => userBookingIds.Contains(p.BookingId))
                .ToListAsync();
            _context.Payments.RemoveRange(payments);

            // حذف السيارات المملوكة
            var cars = await _context.Cars.Where(c => c.OwnerId == user.Id).ToListAsync();
            _context.Cars.RemoveRange(cars);

            // تحديث المراجعات
            var reviews = await _context.Reviews.Where(r => r.UserId == user.Id).ToListAsync();
            foreach (var review in reviews)
            {
                review.UserId = null;
                review.AuthorName = "Deleted User";
            }

            // حذف المستخدم (Soft Delete)
            await _signInManager.SignOutAsync();
            user.IsDeleted = true;
            // _context.Users.Remove(user); // حذف فعلي (تم التعطيل)

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your account has been deleted.";
            return RedirectToAction("Index", "Home");
        }

        // POST: Account/MakeMeAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeMeAdmin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                user.Role = UserRole.Admin;
                await _userManager.UpdateAsync(user);
            }
            TempData["SuccessMessage"] = "You are now an Admin.";
            return RedirectToAction("MyAccount");
        }

        // POST: Account/RemoveProfileImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProfileImage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            // حذف الصورة من السيرفر إذا لم تكن الصورة الافتراضية
            if (!string.IsNullOrEmpty(user.ProfileImageUrl) && user.ProfileImageUrl != "default.png")
            {
                var fileName = Path.GetFileName(user.ProfileImageUrl);
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles", fileName);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            // إعادة تعيين الصورة إلى الافتراضية (store filename only)
            user.ProfileImageUrl = "default.png";
            await _userManager.UpdateAsync(user);
            TempData["SuccessMessage"] = "Profile picture removed.";
            return RedirectToAction("EditProfile");
        }
    }
}
