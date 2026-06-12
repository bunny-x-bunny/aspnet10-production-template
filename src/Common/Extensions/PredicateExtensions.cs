using Common.Enum;
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using static System.Linq.Expressions.Expression;

namespace Common.Extensions {
    public static class PredicateExtensions {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second) {
            var invokedExpr = Invoke(second, first.Parameters.Cast<Expression>());
            return Lambda<Func<T, bool>>(AndAlso(first.Body, invokedExpr), first.Parameters);
        }

        public static Expression<Func<T, bool>> StringOp<T>(this Expression<Func<T, bool>> predicate, StringOp op, Expression<Func<T, string?>> second, string value) {
            var invoked_expr = Invoke(second, predicate.Parameters.Cast<Expression>());
            var u = Parameter(typeof(T), "entity");
            var param = Constant(value);
            var param_lower = Constant(value.ToLower());

            MethodInfo contains = typeof(string).GetMethod("Contains", [typeof(string)])!;
            MethodInfo starts_with = typeof(string).GetMethod("StartsWith", [typeof(string)])!;
            MethodInfo ends_with = typeof(string).GetMethod("EndsWith", [typeof(string)])!;
            MethodInfo is_null_or_empty = typeof(string).GetMethod("IsNullOrEmpty", [typeof(string)])!;
            MethodInfo to_lower = typeof(string).GetMethod("ToLower", [])!;

            Expression op_expr = op switch {
                Enum.StringOp.Equals => Equal(invoked_expr, param),
                Enum.StringOp.NotEquals => NotEqual(invoked_expr, param),
                Enum.StringOp.Contains => Call(Call(invoked_expr, to_lower), contains, param_lower),
                Enum.StringOp.NotContains => Not(Call(Call(invoked_expr, to_lower), contains, param_lower)),
                Enum.StringOp.StartsWith => Call(Call(invoked_expr, to_lower), starts_with, param_lower),
                Enum.StringOp.EndsWith => Call(Call(invoked_expr, to_lower), ends_with, param_lower),
                Enum.StringOp.IsEmpty => Call(is_null_or_empty, invoked_expr),
                Enum.StringOp.IsNotEmpty => Not(Call(is_null_or_empty, invoked_expr))
            };
            return predicate.And(Lambda<Func<T, bool>>(op_expr, u));
        }

        public static Expression<Func<T, bool>> OrdinalOp<T, TValue>(this Expression<Func<T, bool>> predicate, OrdinalOp op, Expression<Func<T, TValue>> second, TValue value) {
            var invoked_expr = Invoke(second, predicate.Parameters.Cast<Expression>());
            var u = Parameter(typeof(T), "entity");
            var param = Constant(value);

            Expression op_expr = op switch {
                Enum.OrdinalOp.GreaterThan => GreaterThan(invoked_expr, param),
                Enum.OrdinalOp.GreaterThanOrEqual => GreaterThanOrEqual(invoked_expr, param),
                Enum.OrdinalOp.Equal => Equal(invoked_expr, param),
                Enum.OrdinalOp.LessThanOrEqual => LessThanOrEqual(invoked_expr, param),
                Enum.OrdinalOp.LessThan => LessThan(invoked_expr, param)
            };

            return predicate.And(Lambda<Func<T, bool>>(op_expr, u));
        }

        public static Expression<Func<T, bool>> SetXSetOp<T, TValue>(this Expression<Func<T, bool>> predicate, SetOp op, Expression<Func<T, IEnumerable<TValue>>> second, IEnumerable<TValue> value) {
            var invoked_expr = Invoke(second, predicate.Parameters.Cast<Expression>());
            var u = Parameter(typeof(T), "entity");
            var param = Constant(value);

            Expression op_expr = op switch {
                SetOp.NotEmpty => EnumerableAny(invoked_expr),
                SetOp.Empty => Not(EnumerableAny(invoked_expr)),
                SetOp.Subset => Equal(EnumerableCount(invoked_expr, (Expression<Func<TValue, bool>>)(l => value.Contains(l))), Constant(value.Count())),
                SetOp.Intersect => GreaterThan(EnumerableCount(invoked_expr, (Expression<Func<TValue, bool>>)(l => value.Contains(l))), Constant(0))
            };

            return predicate.And(Lambda<Func<T, bool>>(op_expr, u));
        }

        private static MethodBase GetGenericMethod(Type type, string name, Type[] typeArgs, Type[] argTypes, BindingFlags flags) {
            int typeArity = typeArgs.Length;
            var methods = type.GetMethods()
                .Where(m => m.Name == name)
                .Where(m => m.GetGenericArguments().Length == typeArity)
                .Select(m => m.MakeGenericMethod(typeArgs));

            return Type.DefaultBinder.SelectMethod(flags, methods.ToArray(), argTypes, null)!;
        }

        private static bool IsIEnumerable(Type type) {
            return type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        private static Type GetIEnumerableImpl(Type type) {
            // Get IEnumerable implementation. Either type is IEnumerable<T> for some T, 
            // or it implements IEnumerable<T> for some T. We need to find the interface.
            if (IsIEnumerable(type))
                return type;
            Type[] t = type.FindInterfaces((m, o) => IsIEnumerable(m), null);
            Debug.Assert(t.Length == 1);
            return t[0];
        }

        private static MethodCallExpression EnumerableAny(Expression collection, Expression? predicate = null) {
            Type t_collection = GetIEnumerableImpl(collection.Type);
            collection = Convert(collection, t_collection);
            Type t_value = t_collection.GetGenericArguments()[0];
            Type t_pred = typeof(Func<,>).MakeGenericType(t_value, typeof(bool));
            // Enumerable.Any<T>(IEnumerable<T>, Func<T,bool>)
            MethodInfo method = (MethodInfo)GetGenericMethod(
                typeof(Enumerable), 
                "Any", 
                [t_value],
                [t_collection, t_pred], 
                BindingFlags.Static
            );
            return Call(method, collection, predicate ?? Lambda(Constant(true), Parameter(t_value)));
        }

        private static MethodCallExpression EnumerableCount(Expression collection, Expression? predicate = null) {
            Type t_collection = GetIEnumerableImpl(collection.Type);
            collection = Convert(collection, t_collection);
            Type t_value = t_collection.GetGenericArguments()[0];
            Type t_pred = typeof(Func<,>).MakeGenericType(t_value, typeof(bool));
            // Enumerable.Count<T>(IEnumerable<T>, Func<T,bool>)
            MethodInfo method = (MethodInfo)GetGenericMethod(
                typeof(Enumerable),
                "Count",
                [t_value],
                [t_collection, t_pred],
                BindingFlags.Static
            );
            return Call(method, collection, predicate ?? Lambda(Constant(true), Parameter(t_value)));
        }

        private static MethodCallExpression EnumerableExcept(Expression collection, Expression value) {
            Type t_collection = GetIEnumerableImpl(collection.Type);
            collection = Convert(collection, t_collection);
            Type t_value = t_collection.GetGenericArguments()[0];
            // Enumerable.Except<T>(IEnumerable<T>, IEnumerable<T> second)
            MethodInfo method = (MethodInfo)GetGenericMethod(
                typeof(Enumerable), 
                "Except", 
                [t_value],
                [t_collection, t_collection], 
                BindingFlags.Static
            );
            return Call(method, collection, value);
        }

        private static MethodCallExpression EnumerableIntersect(Expression collection, Expression value) {
            Type t_collection = GetIEnumerableImpl(collection.Type);
            collection = Convert(collection, t_collection);
            Type t_value = t_collection.GetGenericArguments()[0];
            // Enumerable.Except<T>(IEnumerable<T>, IEnumerable<T> second)
            MethodInfo method = (MethodInfo)GetGenericMethod(
                typeof(Enumerable),
                "Intersect",
                [t_value],
                [t_collection, t_collection],
                BindingFlags.Static
            );
            return Call(method, collection, value);
        }
    }
}
