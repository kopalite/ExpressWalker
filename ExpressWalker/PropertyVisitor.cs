using System;
using System.Linq.Expressions;

namespace ExpressWalker
{
    public interface IPropertyVisitor<TElement>
    {
        Type ElementType { get; }

        Type PropertyType { get; }

        string PropertyName { get; }
        
        void Visit(TElement element);
    }

    internal class PropertyVisitor<TElement>
    {
        public Type ElementType { get { return typeof(TElement); } }
    }

    internal class PropertyVisitor<TElement, TProperty> : PropertyVisitor<TElement>, IPropertyVisitor<TElement>
    {
        public Type PropertyType { get { return typeof(TProperty); } }

        public string PropertyName { get; }

        public ExpressAccessor PropertyAccessor { get; }

        private Func<TProperty, TProperty> _getNewValue;

        internal PropertyVisitor(string propertyName, Expression<Func<TProperty, TProperty>> getNewValue)
        {
            PropertyName = propertyName;

            PropertyAccessor = ExpressAccessor.Create(typeof(TElement), typeof(TProperty), propertyName);

            if (getNewValue != null)
            {
                _getNewValue = getNewValue.Compile();
            }
        }
        
        public void Visit(TElement element)
        {
            if (_getNewValue != null)
            {
                var currentValue = PropertyAccessor.Get(element);
                var newValue = _getNewValue((TProperty)currentValue);
                PropertyAccessor.Set(element, newValue);
            }
        }
    }

    internal class Exp
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
    }
}
