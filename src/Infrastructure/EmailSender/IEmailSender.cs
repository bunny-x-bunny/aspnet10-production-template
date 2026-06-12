using System.ComponentModel.DataAnnotations;

namespace Infrastructure.EmailSender {
    public interface IEmailSender {
        Task SendEmailAsync([EmailAddress] string recipient, string subject, string message, bool is_html = false);
    }
}