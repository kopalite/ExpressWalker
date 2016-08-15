using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressWalker.Helpers
{
    internal sealed class Util
    {
        #region [ Expression based extractions ]

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

        #endregion

        #region [ Simple type validation ]

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
            if (type.IsPrimitive || type.IsEnum || _valueTypes.Contains(type) || Convert.GetTypeCode(type) != TypeCode.Object)
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

        #endregion

        #region [ Collection type validation ]

        public static bool IsIEnumerable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        public static bool ImplementsIEnumerable(Type type)
        {
            return type.GetInterfaces().Any(IsIEnumerable);
        }

        public static Type GetItemsType(Type type)
        {
            if (IsIEnumerable(type))
            {
                return type.GenericTypeArguments.FirstOrDefault();   
            }
            else if (ImplementsIEnumerable(type))
            {
                return GetItemsType(type.GetInterfaces().FirstOrDefault(IsIEnumerable));
            }

            return null;
        }

        #endregion

        #region [ Dictionary type validation ]

        public static bool IsDictionary(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }

        #endregion

        #region [ Constructor validation ]

        public static bool HasParameterlessCtor(Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool HasCollectionCtor(Type type, Type ctorParamType)
        {
            Func<ConstructorInfo, Type> getParameterType = c => c.GetParameters().First().ParameterType;
            return type.GetConstructors().Any(c => c.GetParameters().Length == 1 && getParameterType(c).Equals(ctorParamType) &&
                                                   (IsIEnumerable(getParameterType(c)) || 
                                                    ImplementsIEnumerable(getParameterType(c))));
        }

        public static ConstructorInfo GetCollectionCtor(Type type, Type ctorParamType)
        {
            Func<ConstructorInfo, Type> getParameterType = c => c.GetParameters().First().ParameterType;
            return type.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 1 && getParameterType(c).Equals(ctorParamType) &&
                                                             (IsIEnumerable(getParameterType(c)) ||
                                                              ImplementsIEnumerable(getParameterType(c))));
        }

        #endregion
    }
}
