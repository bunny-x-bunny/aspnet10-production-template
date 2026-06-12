using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Application.DTO.Cat {
  public class UpdateCat {
    [Description("Название категории")]
    [MaxLength(255)]
    public required string Name { get; set; }

    public Domain.Models.Cat UpdateEntity(Domain.Models.Cat entity) {
      entity.Name = Name;
      return entity;
    }
  }
}
