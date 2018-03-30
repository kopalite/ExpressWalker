using ExpressWalker.Visitors;
using System;
using System.Linq.Expressions;

namespace ExpressWalker.Factories
{
    public interface IVisitorsFactory
    {
        IVisitorsFactory Category(string category);

        IVisitorsFactory ForProperty<TPropertyType>(Expression<Func<TPropertyType, object, TPropertyType>> getNewValue);

        IVisitorsFactory ForProperty<TElementType, TPropertyType>(Expression<Func<TElementType, object>> propertyName,
                                                                  Expression<Func<TPropertyType, object, TPropertyType>> getNewValue);

        IVisitor GetVisitor(string category, Type type);
    }
}
