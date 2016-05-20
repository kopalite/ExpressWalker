using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressWalker
{
    public interface IElementVisitor
    {
        Type ElementType { get; }

        string ElementName { get; }

        object Extract(object parent);

        void Visit(object element);
    }

    public interface IElementVisitor<TElement> : IElementVisitor
    {

    }

    internal partial class ElementVisitor<TElement> : IElementVisitor<TElement>
    {
        public Type ElementType { get { return typeof(TElement); } }

        public string ElementName { get; }

        public ExpressAccessor ElementAccessor { get; } 

        private HashSet<IElementVisitor> ElementVisitors { get; }

        private HashSet<IPropertyVisitor<TElement>> PropertyVisitors { get; }

        public ElementVisitor(Type ownerType) : this(ownerType, null)
        {
            
        }

        public ElementVisitor(Type ownerType, string elementName) 
        {
            ElementVisitors = new HashSet<IElementVisitor>();

            PropertyVisitors = new HashSet<IPropertyVisitor<TElement>>();

            ElementName = elementName;

            if (!string.IsNullOrWhiteSpace(elementName))
            {
                ElementAccessor = ExpressAccessor.Create(ownerType, typeof(TElement), elementName);
            }
        }

        public object Extract(object parent)
        {
            return ElementAccessor.Get(parent);
        }

        public void Visit(TElement element)
        {
            foreach (var propertyVisitor in PropertyVisitors)
            {
                propertyVisitor.Visit(element);
            }

            foreach (var elementVisitor in ElementVisitors)
            {
                var childElement = elementVisitor.Extract(element);

                elementVisitor.Visit(childElement);
            }
        }

        public void Visit(object element)
        {
            if (element == null || !(element is TElement))
            {
                return;
            }

            Visit((TElement)element);
        }
    }

    internal partial class ElementVisitor<TElement>
    {
        public IElementVisitor<TChildElement> AddElementVisitor<TChildElement>(string elementName)
        {
            if (ElementVisitors.Any(ev => ev.ElementType == typeof(TChildElement) && ev.ElementName == elementName))
            {
                throw new ArgumentException(string.Format("Element visitor for type '{0}' and name '{1}' is already added!", typeof(TElement), elementName));
            }

            var elementVisitor = new ElementVisitor<TChildElement>(typeof(TElement), elementName);
            ElementVisitors.Add(elementVisitor);
            return elementVisitor;
        }

        public IElementVisitor<TElement> AddPropertyVisitor<TProperty>(string propertyName, Expression<Func<TProperty, TProperty>> newValue)
        {
            if (PropertyVisitors.Any(pv => pv.ElementType == typeof(TElement) && pv.PropertyName == propertyName))
            {
                throw new ArgumentException(string.Format("Property Visitor for type '{0}' and name '{1}' is already added!", typeof(TElement), propertyName));
            }

            var elementVisitor = new PropertyVisitor<TElement, TProperty>(propertyName, newValue);
            PropertyVisitors.Add(elementVisitor);
            return this;
        }
    }
}
