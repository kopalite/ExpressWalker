using System;
using System.Linq.Expressions;

namespace ExpressWalker
{
    internal class ElementTarget
    {
        public Type ElementType { get; }
        
        public ElementTarget(Type elementType)
        {
            ElementType = elementType;
        }
    }

    internal abstract class PropertyTarget
    {
        public Type ElementType { get; }

        public string PropertyName { get; }

        public Type PropertyType { get; }

        // An object convertible to Func<TPropertyType, TPropertyType> where TPropertyType is specified in derived class.
        public Expression GetOldValue { get; protected set; }

        // An object convertible to Action<TPropertyType> where TPropertyType is specified in derived class.
        public Expression GetNewValue { get; protected set; }

        public PropertyTarget(Type elementType, Type propertyType, string propertyName)
        {
            ElementType = elementType;
            PropertyType = propertyType;
            PropertyName = propertyName;
        }
    }

    internal class PropertyTarget<TPropertyType> : PropertyTarget
    {

        public PropertyTarget(Type elementType,
                              Type propertyType,
                              string propertyName,
                              Expression<Action<TPropertyType, object>> getOldValue,
                              Expression<Func<TPropertyType, object, TPropertyType>> getNewValue) 
            : base(elementType, propertyType, propertyName)
        {
            GetOldValue = getOldValue;
            GetNewValue = getNewValue;
        }
    }
}
