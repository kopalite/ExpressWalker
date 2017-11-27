using ExpressWalker.Cloners;
using System;

namespace ExpressWalker.Visitors
{
    internal sealed class DictionaryVisitor<TElement> : ElementVisitor<TElement>
    {
        public DictionaryVisitor(Type ownerType, 
                                 Type dictionaryType, 
                                 string elementName = null, 
                                 PropertyGuard guard = null, 
                                 bool supportsCloning = true) : base(guard, supportsCloning)
        {
            ElementName = elementName;
                
            if (!string.IsNullOrWhiteSpace(elementName))
            {
                _elementAccessor = ExpressAccessor.Create(ownerType, dictionaryType, elementName);
            }

            if (SupportsCloning)
            {
                _elementCloner = ClonerBase.Create(dictionaryType);
            }
        }
    }
}
