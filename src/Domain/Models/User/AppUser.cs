using Domain.Enum;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models.User {
  public class AppUser : IdentityUser<Guid> {
    [Description("Роль пользователя")]
    public required Role Role { get; set; }
    [MaxLength(255)]
    [Description("Полное имя")]
    public required string FullName { get; set; }
    [Description("Аватар")]
    public Guid? AvatarId { get; set; }
    public File? Avatar { get; set; }
  }
}
