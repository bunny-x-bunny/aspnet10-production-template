using Domain.Models.User;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence;
using AppEmailSender = global::Infrastructure.EmailSender.IEmailSender;
using IdentityUiEmailSender = Microsoft.AspNetCore.Identity.UI.Services.IEmailSender;

namespace API.Integration.Infrastructure {
  public class TestApiFactory(string connectionString, string uploadPath) : WebApplicationFactory<Program> {
    private readonly string connectionString = connectionString;
    private readonly string uploadPath = uploadPath;

    protected override void ConfigureWebHost(IWebHostBuilder builder) {
      builder.UseEnvironment("Development");
      builder.UseSetting("ConnectionStrings:PostgresConnection", connectionString);
      builder.UseSetting("AppSettings:UploadPath", uploadPath);

      builder.ConfigureServices(services => {
        // Replace SMTP-bound email senders with no-op fakes so tests don't
        // require a working mail server.
        services.RemoveAll<AppEmailSender>();
        services.RemoveAll<IdentityUiEmailSender>();
        services.RemoveAll<IEmailSender<AppUser>>();

        services.AddSingleton<AppEmailSender, NoopEmailSender>();
        services.AddSingleton<IdentityUiEmailSender, NoopEmailSender>();
        services.AddSingleton<IEmailSender<AppUser>, NoopIdentityEmailSender>();
      });
    }

    public async Task ApplyMigrationsAsync() {
      var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql(connectionString)
        .Options;
      await using var context = new AppDbContext(options);
      await context.Database.MigrateAsync();
    }
  }

  internal class NoopEmailSender : AppEmailSender, IdentityUiEmailSender {
    public Task SendEmailAsync(string recipient, string subject, string message, bool is_html = false)
      => Task.CompletedTask;
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
      => Task.CompletedTask;
  }

  internal class NoopIdentityEmailSender : IEmailSender<AppUser> {
    public Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink) => Task.CompletedTask;
    public Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink) => Task.CompletedTask;
    public Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode) => Task.CompletedTask;
  }
}
