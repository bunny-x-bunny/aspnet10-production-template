using Application.DTO.File;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Application.DTO.Cat {
  public class GetCatFamilyEntry {
    public required Guid Id { get; set; }
    [Description("Название категории")]
    [MaxLength(255)]
    public required string Name { get; set; }
    public required int LeftEar { get; set; }
    public required int RightEar { get; set; }
    public ICollection<GetCatFamilyEntry> Children { get; set; } = [];
  }
}
