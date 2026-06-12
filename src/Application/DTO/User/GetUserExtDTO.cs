using Application.DTO.File;
using Domain.Enum;
using Domain.Models.User;
using System.ComponentModel;

namespace Application.DTO.User {
  public class GetUserExtDTO : GetUserBasicDTO {
    [Description("Email confirmed")]
    public required bool EmailConfirmed { get; set; }
    //[Description("Permissions")]
    //public required ISet<Permission> Permissions { get; set; }

    public static new GetUserExtDTO FromEntity<T>(
      T entity
    ) where T : AppUser => new() {
      Id = entity.Id,
      Email = entity.Email ?? "",
      EmailConfirmed = entity.EmailConfirmed,
      Role = entity.Role,
      PhoneNumber = entity.PhoneNumber,
      FullName = entity.FullName,
      Avatar = entity.Avatar != null ? GetFileDTO.FromEntity(entity.Avatar) : null,
    };
  }
}
