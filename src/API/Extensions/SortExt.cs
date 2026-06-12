using Application.DTO;
using Common.Extensions;
using Domain.Enum;
using MR.EntityFrameworkCore.KeysetPagination;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using static System.Linq.Expressions.Expression;

namespace API.Extensions {
    public static class SortExt {
        public static Expression<Action<KeysetPaginationBuilder<T>>> ToExpression<T>(this SortModel sort) {
            var t_par = Parameter(typeof(T));
            var id_prop = typeof(T).GetProperty("Id");

            // resolve primary sort column
            var sort_prop = sort.SortBy switch {
                string sort_by 
                    when sort_by.Split('.').Select(x => x.FirstCharToUpper()) is var @props
                    && @props.Any()
                    => props.Skip(1)
                        .Aggregate( // resolve nested fields
                            Property(t_par, props.First()!), 
                            (a, x) => Property(a, x.FirstCharToUpper()!)
                        ),
                // use first property of the model by default
                _ => Property(t_par, id_prop ?? typeof(T).GetProperties().First())
            };

            // secondary sort column
            var secondary_sort_prop = sort_prop.Member.Name is not "Id" && id_prop is not null ? Property(t_par, id_prop) : null;
            var sort_desc = Constant((sort.SortOrder ?? SortOrder.Desc) == SortOrder.Desc);

            var builder_par = Parameter(typeof(KeysetPaginationBuilder<T>));
            var configure_column = typeof(KeysetPaginationBuilder<T>).GetMethod("ConfigureColumn");
            var lambda_inner = Call(
                builder_par,
                configure_column!.MakeGenericMethod([((PropertyInfo)sort_prop.Member).PropertyType]),
                [Lambda(sort_prop, t_par), sort_desc]
            );
            if (secondary_sort_prop is not null)
                lambda_inner = Call(
                    lambda_inner,
                    configure_column!.MakeGenericMethod([((PropertyInfo)secondary_sort_prop.Member).PropertyType]),
                    [Lambda(secondary_sort_prop, t_par), sort_desc]
                );
            return Lambda<Action<KeysetPaginationBuilder<T>>>(
                lambda_inner,
                builder_par
            );
        }
    }
}
