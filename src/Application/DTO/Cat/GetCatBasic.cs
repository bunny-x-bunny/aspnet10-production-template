using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Application.DTO.Cat {
  public class GetCatBasic {
    public required Guid Id { get; set; }
    [Description("Название категории")]
    [MaxLength(255)]
    public required string Name { get; set; }
  }
}
