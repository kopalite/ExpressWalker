using System;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressWalker
{
    public static class ManualWalker
    {
        public static IElementVisitor<TElement> Create<TElement>() where TElement : class, new()
        {
            return new ElementVisitor<TElement>(typeof(TElement));
        }

        public static IElementVisitor<TElement> Element<TElement, TChildElement>(this IElementVisitor<TElement> element,
                                                                                 Expression<Func<TElement, object>> elementName) 
            where TElement : class, new() 
            where TChildElement : class, new()
            
        {
            return element.Element<TElement, TChildElement>(elementName, null);
        }

        public static IElementVisitor<TElement> Element<TElement, TChildElement>(this IElementVisitor<TElement> element, 
                                                                                 Expression<Func<TElement, object>> elementName,
                                                                                 Expression<Action<IElementVisitor<TChildElement>>> buildAction)
            where TElement : class, new()
            where TChildElement : class, new()
        {
            var myElement = (ElementVisitor<TElement>)element;
            var extractedName = Util.NameOf(elementName);
            var childElement = myElement.AddElementVisitor<TChildElement>(extractedName);

            if (buildAction != null)
            {
                buildAction.Compile()(childElement);
            }

            return element;
        }

        public static IElementVisitor<TElement> Property<TElement, TProperty>(this IElementVisitor<TElement> element,
                                                                              Expression<Func<TElement, object>> propertyName,
                                                                              Expression<Action<TProperty>> getOldValue,
                                                                              Expression<Func<TProperty, object, TProperty>> getNewValue) 
            where TElement : class, new()
            
        {
            var myElement = (ElementVisitor<TElement>)element;
            var extractedName = Util.NameOf(propertyName);
            var childElement = myElement.AddPropertyVisitor(extractedName, getOldValue, getNewValue);
            return element;
        }
    }
}
