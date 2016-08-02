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

        private Action<TProperty> _getOldValue;

        private Func<TProperty, object, TProperty> _getNewValue;

        private object _metadata;
        
        internal PropertyVisitor(string propertyName, Expression<Action<TProperty>> getOldValue, Expression<Func<TProperty, object, TProperty>> getNewValue, object metadata)
        {
            PropertyName = propertyName;

            _propertyAccessor = ExpressAccessor.Create(typeof(TElement), typeof(TProperty), propertyName);

            if (getOldValue != null)
            {
                _getOldValue = getOldValue.Compile();
            }

            if (getNewValue != null)
            {
                _getNewValue = getNewValue.Compile();
            }

            _metadata = metadata;
        }
        
        public void Visit(TElement element, TElement blueprint)
        {
            if (_getOldValue != null)
            {
                var currentValue = _propertyAccessor.Get(element);
                _getOldValue((TProperty)currentValue);
            }

            if (_getNewValue != null)
            {
                var currentValue = _propertyAccessor.Get(element);
                var newValue = _getNewValue((TProperty)currentValue, _metadata);
                _propertyAccessor.Set(element, newValue);

                if (blueprint != null)
                {
                    _propertyAccessor.Set(blueprint, newValue);
                }
            }
        }
    }
}
