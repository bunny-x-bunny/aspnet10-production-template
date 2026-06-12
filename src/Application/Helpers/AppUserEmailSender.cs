using Domain.Models.User;
using Infrastructure.EmailSender;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace Application.Helpers {
    public class AppUserEmailSender {
        readonly static string confirmEmailEndpointName = "/Auth/confirm_email";
        public required IEmailSender email_sender;
        public required LinkGenerator link_generator;

        public async Task SendConfirmationEmailAsync(AppUser user, UserManager<AppUser> userManager, HttpContext context, string email, bool isChange = false) {
            if (confirmEmailEndpointName is null) {
                throw new NotSupportedException("No email confirmation endpoint was registered!");
            }

            var code = isChange
                ? await userManager.GenerateChangeEmailTokenAsync(user, email)
                : await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var userId = await userManager.GetUserIdAsync(user);
            var routeValues = new RouteValueDictionary() {
                ["userId"] = userId,
                ["code"] = code,
            };

            if (isChange) {
                // This is validated by the /confirmEmail endpoint on change.
                routeValues.Add("changedEmail", email);
            }

            var confirmEmailUrl = link_generator.GetUriByName(context, confirmEmailEndpointName, routeValues)
                ?? throw new NotSupportedException($"Could not find endpoint named '{confirmEmailEndpointName}'.");

            var message = string.Format(
                "Подтвердите ваш email перейдя <a href=\"{0}\">по ссылке</a>.", 
                HtmlEncoder.Default.Encode(confirmEmailUrl)
            );
            await email_sender.SendEmailAsync(email, "Подтверждение аккаунта", message, true);
        }
    }
}
