using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.User {
  public interface ICreateUserCredentials {
    [Description("Email")]
    [EmailAddress]
    [Required]
    [MaxLength(255)]
    public string Email { get; set; }
    [Description("Password")]
    [MinLength(8)]
    public string? Password { get; set; }
  }
}
