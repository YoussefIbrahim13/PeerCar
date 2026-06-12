using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Peer_Car.Application.ViewModels;
using Peer_Car.Domain.Entities;

namespace Peer_Car.Application.Interfaces
{
    public interface IAccountService
    {
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task<IdentityResult> RegisterAsync(RegisterViewModel model, Func<User, string, string> linkFactory);
        Task<IdentityResult> ConfirmEmailAsync(Guid userId, string token);
        Task LogoutAsync();
        Task<EditProfileViewModel> GetEditProfileModelAsync(User user);
        Task<IdentityResult> UpdateProfileAsync(User user, EditProfileViewModel model, string? emailConfirmationLink);
        Task<IdentityResult> ConfirmEmailChangeAsync(Guid userId, string newEmail, string token);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<string> GeneratePasswordResetLinkAsync(User user, string scheme);
        Task<IdentityResult> DeleteAccountAsync(User user);
        Task<IdentityResult> MakeAdminAsync(User user);
        Task RemoveProfileImageAsync(User user);

        Task<AuthenticationProperties> ConfigureExternalAuthenticationProperties(string provider, string redirectUrl);
        Task<ExternalLoginInfo?> GetExternalLoginInfoAsync();
        Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey);
        Task<(IdentityResult Result, User User)> CreateUserFromExternalAsync(ExternalLoginInfo info);


        Task SendEmailGenericAsync(string email, string subject, string body);

        Task<EditProfileViewModel?> GetUserProfileForViewingAsync(Guid targetUserId, Guid currentUserId);
    }
}
