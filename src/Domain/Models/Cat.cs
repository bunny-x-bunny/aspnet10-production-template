using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Domain.Models {
  public class Cat {
    [Key]
    public Guid Id { get; init; }
    public Guid? ParentId { get; set; }
    public Cat? Parent { get; set; }
    [Description("Category name")]
    [MaxLength(255)]
    public required string Name { get; set; }
    public required int LeftEar { get; set; }
    public required int RightEar { get; set; }
    public ICollection<Cat> Children { get; set; } = [];

    public bool HasChildren() => RightEar - LeftEar > 1;
    public int ChildCount() => (RightEar - LeftEar) / 2;
  }
}
