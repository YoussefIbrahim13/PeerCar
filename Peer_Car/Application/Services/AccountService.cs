using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Peer_Car.Application.Interfaces;
using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;
using Peer_Car.Domain.Interfaces;
using Peer_Car.Infrastructure.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting; // تأكد من وجود دي عشان الـ Environment
using Microsoft.AspNetCore.Http;

namespace Peer_Car.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return SignInResult.Failed;
            return await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
        }

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model, Func<User, string, string> linkFactory)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = NormalizePhoneNumber(model.PhoneNumber),
                Role = UserRole.Renter,
                DateRegistered = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Renter");
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = linkFactory(user, token);
                await SendTemplatedEmail(user, "Confirm your email", confirmationLink);
            }

            return result;
        }

        // ------------------------------------------------------------------
        // 🛠️ الميثود دي هي اللي كان فيها المشكلة (تم التعديل)
        // ------------------------------------------------------------------
        public async Task<EditProfileViewModel> GetEditProfileModelAsync(User user)
        {
            return new EditProfileViewModel
            {
                UserId = user.Id, // ✅ إضافة الـ ID هنا هو اللي هيظهر الأزرار
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber?.Replace("+20", "") ?? "",
                ProfileImageUrl = user.ProfileImageUrl,
                
                // إضافة روابط المستندات عشان تظهر في صفحة "My Account"
                NationalIdFrontUrl = user.NationalIdFrontUrl,
                NationalIdBackUrl = user.NationalIdBackUrl,
                LicenseFrontUrl = user.LicenseFrontUrl,
                LicenseBackUrl = user.LicenseBackUrl
            };
        }

        public async Task<IdentityResult> UpdateProfileAsync(User user, EditProfileViewModel model, string? emailConfirmationLink)
        {
            user.FirstName = model.FullName.Split(' ')[0];
            user.LastName = model.FullName.Contains(' ') ? model.FullName.Substring(model.FullName.IndexOf(' ') + 1) : "";

            user.PhoneNumber = "+20" + model.PhoneNumber.Trim().Replace(" ", "");

            if (model.ProfileImage != null)
            {
                user.ProfileImageUrl = await UploadImage(model.ProfileImage, user.Id);
            }

            if (model.IdFrontFile != null || model.IdBackFile != null ||
                model.LicenseFrontFile != null || model.LicenseBackFile != null)
            {
                user.DocumentStatus = DocumentStatus.Pending;

                user.IsDocumentsVerified = false;
            }

            if (user.Email != model.Email && !string.IsNullOrEmpty(emailConfirmationLink))
            {
                user.PendingEmail = model.Email;
                await SendTemplatedEmail(user, "Confirm your new email", emailConfirmationLink);
            }

            return await _userManager.UpdateAsync(user);
        }
        public async Task<IdentityResult> DeleteAccountAsync(User user)
        {
          
            user.IsDeleted = true;

         
            string suffix = DateTime.UtcNow.Ticks.ToString();

            
            user.Email = $"deleted_{suffix}_{user.Email}";
            user.UserName = $"deleted_{suffix}_{user.UserName}";
            user.NormalizedEmail = user.Email.ToUpper();
            user.NormalizedUserName = user.UserName.ToUpper();

            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                user.PhoneNumber = $"del_{suffix}_{user.PhoneNumber}";
            }

           
            var reviews = await _context.Reviews.Where(r => r.UserId == user.Id).ToListAsync();
            foreach (var r in reviews)
            {
                r.AuthorName = "Deleted User";
            }

    
            var userCars = await _context.Cars.Where(c => c.OwnerId == user.Id).ToListAsync();
            foreach (var car in userCars)
            {
                car.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> MakeAdminAsync(User user)
        {
            user.Role = UserRole.Admin;
            await _userManager.AddToRoleAsync(user, "Admin");
            return await _userManager.UpdateAsync(user);
        }

        public async Task RemoveProfileImageAsync(User user)
        {
            if (user.ProfileImageUrl != "default.png")
            {
                DeleteOldImage(user.ProfileImageUrl);
                user.ProfileImageUrl = "default.png";
                await _userManager.UpdateAsync(user);
            }
        }

        private string NormalizePhoneNumber(string phone)
        {
            if (phone.StartsWith("+20")) phone = phone.Substring(3);
            if (phone.StartsWith("0")) phone = phone.Substring(1);
            return "+20" + phone;
        }

        private async Task<string> UploadImage(IFormFile file, Guid userId)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"user_{userId}_{DateTime.Now.Ticks}{ext}";
            var path = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }

        private void DeleteOldImage(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == "default.png") return;
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles", fileName);
            if (File.Exists(path)) File.Delete(path);
        }

        private async Task SendTemplatedEmail(User user, string subject, string link)
        {
            var templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "templates", "EmailTemplate.html");
            var body = File.Exists(templatePath)
                ? File.ReadAllText(templatePath).Replace("{{UserName}}", user.FirstName).Replace("{{ConfirmationLink}}", link)
                : $"Please click here to verify: {link}";

            await _emailSender.SendEmailAsync(user.Email!, subject, body);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(Guid userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user == null ? IdentityResult.Failed() : await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<IdentityResult> ConfirmEmailChangeAsync(Guid userId, string newEmail, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user == null ? IdentityResult.Failed() : await _userManager.ChangeEmailAsync(user, newEmail, token);
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            return user == null ? IdentityResult.Failed() : await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        }

        public async Task<string> GeneratePasswordResetLinkAsync(User user, string scheme)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task LogoutAsync() => await _signInManager.SignOutAsync();

        public async Task<AuthenticationProperties> ConfigureExternalAuthenticationProperties(string provider, string redirectUrl)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }

        public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync() => await _signInManager.GetExternalLoginInfoAsync();

        public async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey)
        {
            return await _signInManager.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent: false, bypassTwoFactor: true);
        }

        public async Task<(IdentityResult Result, User User)> CreateUserFromExternalAsync(ExternalLoginInfo info)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "User";
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";

            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                DateRegistered = DateTime.UtcNow,
                Role = UserRole.Renter
            };

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Renter");
                result = await _userManager.AddLoginAsync(user, info);
            }
            return (result, user);
        }

        public async Task SendEmailGenericAsync(string email, string subject, string body) => await _emailSender.SendEmailAsync(email, subject, body);

        public async Task<EditProfileViewModel?> GetUserProfileForViewingAsync(Guid targetUserId, Guid currentUserId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (user == null) return null;

            var isOwnerOfRenter = await _context.Bookings
                .AnyAsync(b => b.Car.OwnerId == currentUserId && b.RenterId == targetUserId);

            return new EditProfileViewModel
            {
                UserId = user.Id, // ✅ موجودة هنا فعلاً
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                NationalIdFrontUrl = isOwnerOfRenter ? user.NationalIdFrontUrl : null,
                NationalIdBackUrl = isOwnerOfRenter ? user.NationalIdBackUrl : null,
                LicenseFrontUrl = isOwnerOfRenter ? user.LicenseFrontUrl : null,
                LicenseBackUrl = isOwnerOfRenter ? user.LicenseBackUrl : null,
                IsOwnerViewing = isOwnerOfRenter
            };
        }
    }
}