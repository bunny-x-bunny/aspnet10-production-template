using Common.Enum;
using Common.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Application.DTO.Cat.Search {
  public class CatSearchModel {
    [Description("🔍 Parent category")]
    public Guid? ParentId { get; set; }
    [Description("🔍 LeftEar")]
    public int? LeftEar { get; set; }
    public OrdinalOp? LeftEarOp { get; set; }
    [Description("🔍 RightEar")]
    public int? RightEar { get; set; }
    public OrdinalOp? RightEarOp { get; set; }
    [Description("🔍 Category name")]
    [MaxLength(255)]
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string? Name { get; set; }
    public StringOp? NameOp { get; set; }

    public Expression<Func<Domain.Models.Cat, bool>> ToPredicate() {
      Expression<Func<Domain.Models.Cat, bool>> predicate = u => true;

      if (ParentId is not null)
        predicate = predicate.And(c => c.ParentId == ParentId);
      if (LeftEar is int @left_ear && LeftEarOp is OrdinalOp @left_ear_op)
        predicate = predicate.OrdinalOp(left_ear_op, c => c.LeftEar, left_ear);
      if (RightEar is int @right_ear && RightEarOp is OrdinalOp @right_ear_op)
        predicate = predicate.OrdinalOp(right_ear_op, c => c.RightEar, right_ear);
      if (Name is not null && NameOp is StringOp @name_op)
        predicate = predicate.StringOp(name_op, c => c.Name, Name);

      return predicate;
    }
  }
}
