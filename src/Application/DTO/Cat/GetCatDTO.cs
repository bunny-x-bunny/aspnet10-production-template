using Application.DTO.File;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Application.DTO.Cat {
  public class GetCatDTO {
    public required Guid Id { get; set; }
    public required Guid? ParentId { get; set; }
    [Description("Category name")]
    [MaxLength(255)]
    public required string Name { get; set; }
    public required int LeftEar { get; set; }
    public required int RightEar { get; set; }

    public static GetCatDTO FromEntity(Domain.Models.Cat entity) => new() {
      Id = entity.Id,
      ParentId = entity.ParentId,
      Name = entity.Name,
      LeftEar = entity.LeftEar,
      RightEar = entity.RightEar,
    };
  }
}
