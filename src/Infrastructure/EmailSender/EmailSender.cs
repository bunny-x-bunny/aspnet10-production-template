using Domain.Models.User;
using NETCore.MailKit.Core;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.EmailSender {
  public class EmailSender(IEmailService email_service) :
    Infrastructure.EmailSender.IEmailSender,
    Microsoft.AspNetCore.Identity.UI.Services.IEmailSender,
    Microsoft.AspNetCore.Identity.IEmailSender<AppUser> 
  {
    private readonly IEmailService email_service = email_service;

    public async Task SendEmailAsync([EmailAddress] string recipient, string subject, string message, bool is_html = false) {
      await email_service.SendAsync(recipient, subject, message, is_html);
    }

    public async Task SendEmailAsync(string recipient, string subject, string message) {
      await SendEmailAsync(recipient, subject, message, true);
    }

    // identity 

    public async Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink) {
      await SendEmailAsync(
        email,
        "Подтверждение аккаунта",
        $"Пожалуйста, подтвердите свой аккаунт, перейдя по <a href=\"{confirmationLink}\">следующей ссылке</a>.",
        true
      );
    }

    public async Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode) {
      await SendEmailAsync(
        email,
        "Сброс пароля",
        $"Ваш код для сброса пароля: <br>" +
        $"<code>{resetCode}</code>",
        true
      );
    }

    public async Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink) {
      await SendEmailAsync(
        email,
        "Сброс пароля",
        $"Пожалуйста, сбросьте свой пароль, перейдя по <a href=\"{resetLink}\">следующей ссылке</a>.",
        true
      );
    }
  }
}
