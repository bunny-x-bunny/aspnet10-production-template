using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models {
  public class AppSettings {
    public required string UploadPath { get; set; }
    public required Authentication Authentication { get; set; }
    public required SMTPSettings SMTP { get; set; }
  }

  public class Authentication {
    public required string JWTIssuer { get; set; }
    public required string JWTAudience { get; set; }
    [MinLength(32)]
    public required string JWTKey { get; set; }
    public required long BearerTokenExpiration { get; set; }
    public required long RefreshTokenExpiration { get; set; }
  }

  public class SMTPSettings {
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string Login { get; set; }
    public required string Password { get; set; }
    public required string SenderName { get; set; }
    public required string SenderEmail { get; set; }
  }
}
