using Application.DTO.File;
using Domain.Enum;
using Domain.Models.User;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.User {
  public class GetUserBasicDTO : UserBasicDTO {
    public required Guid Id { get; set; }
    [Description("Email")]
    [EmailAddress]
    [Required]
    [MaxLength(255)]
    public required string Email { get; set; }
    [Description("Роль пользователя")]
    public required Role Role { get; set; }
    [Description("Аватар")]
    public GetFileDTO? Avatar { get; set; }

    public static GetUserBasicDTO? FromEntity<T>(T entity) where T : AppUser? => 
      entity is null ? null :
      new() {
        Id = entity.Id,
        Email = entity.Email ?? "",
        Role = entity.Role,
        PhoneNumber = entity.PhoneNumber,
        FullName = entity.FullName,
        Avatar = entity.Avatar != null ? GetFileDTO.FromEntity(entity.Avatar) : null
    };
  }
}
