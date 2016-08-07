using System;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressWalker.Helpers
{
    internal sealed class Util
    {
        public static string NameOf<TType>(Expression<Func<TType, object>> expression)
        {
            return NameOf((Expression)expression);
        }

        private static string NameOf(Expression expression)
        {
            var lambda = expression as LambdaExpression;
            if (lambda != null)
            {
                return NameOf(lambda.Body);
            }

            var unary = expression as UnaryExpression;
            if (unary != null)
            {
                return NameOf(unary.Operand);
            }

            var member = expression as MemberExpression;
            if (member != null)
            {
                return member.Member.Name;
            }

            throw new ArgumentException("Must be unary expression or member expression!");
        }

        public static Func<TEntity> Constructor<TEntity>()
        {
            var body = Expression.New(typeof(TEntity));
            var lambda = Expression.Lambda<Func<TEntity>>(body);
            return lambda.Compile();
        }

        private static Type[] _valueTypes = new Type[]
        {
             typeof(Enum),
             typeof(string),
             typeof(decimal),
             typeof(DateTime),
             typeof(DateTimeOffset),
             typeof(TimeSpan),
             typeof(Guid)
        };

        public static bool IsSimpleType(Type type)
        {
            if (type.IsPrimitive || _valueTypes.Contains(type) || Convert.GetTypeCode(type) != TypeCode.Object)
            {
                return true;
            }

            var isNullableType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

            if (isNullableType)
            {
                var genericTypeArgs = type.GetGenericArguments().First();

                return IsSimpleType(genericTypeArgs);
            }

            return false;
        }
    }
}
