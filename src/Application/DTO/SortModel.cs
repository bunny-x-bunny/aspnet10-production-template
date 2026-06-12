using Common.Enum;
using Domain.Enum;
using System.ComponentModel;
//using System.Linq.Dynamic.Core;

namespace Application.DTO {
    public class SortModel {
        [Description("📈 Sort field")]
        public string? SortBy { get; set; }
        [Description("📈 Sort direction")]
        public SortOrder? SortOrder { get; set; }
    }

    /*public static class SortModelExt {
        public static IQueryable<T> OrderDyn<T>(this IQueryable<T> query, SortModel sort)
            => query.OrderBy(sort.SortBy + " " + sort.SortOrder switch {
                SortOrder.Asc => "asc",
                SortOrder.Desc => "desc"
            });
    }*/
}
