using Domain.Enum;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models.User {
  public class AppUser : IdentityUser<Guid> {
    [Description("User role")]
    public required Role Role { get; set; }
    [MaxLength(255)]
    [Description("Full name")]
    public required string FullName { get; set; }
    [Description("Avatar")]
    public Guid? AvatarId { get; set; }
    public File? Avatar { get; set; }
  }
}
