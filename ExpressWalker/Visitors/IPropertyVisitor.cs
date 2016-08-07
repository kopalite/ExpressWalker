using System;

namespace ExpressWalker.Visitors
{
    public interface IPropertyVisitor<TElement>
    {
        Type ElementType { get; }

        Type PropertyType { get; }

        string PropertyName { get; }

        PropertyValue Visit(TElement element, TElement blueprint);
    }
}
