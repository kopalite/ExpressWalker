using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

        public static Type TypeOf<TType>(Expression<Func<TType, object>> expression)
        {
            var lambda = expression as LambdaExpression;
            if (lambda != null)
            {
                var member = lambda.Body as MemberExpression;
                if (member != null)
                {
                    var property = member.Member as PropertyInfo;
                    if (property != null)
                    {
                        return property.PropertyType;
                    }

                    var field = member.Member as FieldInfo;
                    if (field != null)
                    {
                        return field.FieldType;
                    }
                }
            }

            throw new ArgumentException("Must be member expression of class' field property!");
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
