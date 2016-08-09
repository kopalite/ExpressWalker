using ExpressWalker.Cloners;
using System;

namespace ExpressWalker.Visitors
{
    internal sealed class CollectionVisitor<TElement> : ElementVisitor<TElement>
    {
        public CollectionVisitor(Type ownerType, Type collectionType) : this(ownerType, collectionType, null)
        {

        }

        public CollectionVisitor(Type ownerType, Type collectionType, string elementName) : base()
        {
            ElementName = elementName;
                
            if (!string.IsNullOrWhiteSpace(elementName))
            {
                _elementAccessor = ExpressAccessor.Create(ownerType, collectionType, elementName);
            }
            
            _elementCloner = ClonerBase.Create(collectionType);
        }
    }
}
