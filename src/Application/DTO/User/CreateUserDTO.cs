using Domain.Enum;
using Domain.Models.User;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.User {
  public class CreateUserDTO : UpdateUserDTO, ICreateUserCredentials {
    [Description("Email")]
    [EmailAddress]
    [Required]
    [MaxLength(255)]
    public required string Email { get; set; }
    [Description("Password  \n" +
      "`null` - will be generated automatically")]
    [MinLength(8)]
    public string? Password { get; set; }


    public AppUser ToEntity() => new() {
      Role = Domain.Enum.Role.User,

      PhoneNumber = PhoneNumber,
      FullName = FullName,
    };
  }
}
