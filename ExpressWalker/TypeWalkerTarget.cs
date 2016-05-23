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
        
        // An object convertible to Func<TPropertyType, TPropertyType> where TPropertyType is specified in derived class.
        public abstract Expression GetNewValue { get; }

        public PropertyTarget(Type elementType, string propertyName)
        {
            ElementType = elementType;

            PropertyName = propertyName;
        }
    }

    internal class PropertyTarget<TPropertyType> : PropertyTarget
    {
        public override Expression GetNewValue { get; }

        public PropertyTarget(Type elementType,  
                              string propertyName, 
                              Expression<Func<TPropertyType, TPropertyType>> getNewValue) : base(elementType, propertyName)
        {
            GetNewValue = getNewValue;
        }
    }
}
