using System;
using System.Linq.Expressions;

namespace ExpressWalker
{
    public interface IPropertyVisitor<TElement>
    {
        Type ElementType { get; }

        Type PropertyType { get; }

        string PropertyName { get; }
        
        void Visit(TElement element, TElement blueprint);
    }

    internal class PropertyVisitor<TElement>
    {
        public Type ElementType { get { return typeof(TElement); } }
    }

    internal class PropertyVisitor<TElement, TProperty> : PropertyVisitor<TElement>, IPropertyVisitor<TElement>
    {
        public Type PropertyType { get { return typeof(TProperty); } }

        public string PropertyName { get; }

        private ExpressAccessor _propertyAccessor;

        private Func<TProperty, TProperty> _getNewValue;

        internal PropertyVisitor(string propertyName, Expression<Func<TProperty, TProperty>> getNewValue)
        {
            PropertyName = propertyName;

            _propertyAccessor = ExpressAccessor.Create(typeof(TElement), typeof(TProperty), propertyName);

            if (getNewValue != null)
            {
                _getNewValue = getNewValue.Compile();
            }
        }
        
        public void Visit(TElement element, TElement blueprint)
        {
            if (_getNewValue != null)
            {
                var currentValue = _propertyAccessor.Get(element);
                var newValue = _getNewValue((TProperty)currentValue);
                _propertyAccessor.Set(element, newValue);
                _propertyAccessor.Set(blueprint, newValue);
            }
        }
    }

    
}
