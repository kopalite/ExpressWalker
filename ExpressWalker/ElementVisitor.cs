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

        object SetCopy(object parent, object element);

        void Visit(object element, object blueprint);
    }

    public interface IElementVisitor<TElement> : IElementVisitor
    {

    }

    internal abstract class ElementVisitor
    {
       

        public abstract Type ElementType { get; }

        public string ElementName { get; protected set; }

        

        public abstract ElementVisitor AddElement(Type elementType, string elementName);

        //getNewValue is convertible to Expression<Func<TPropertyType, TPropertyType>> where TPropertyType is specified in derived class.
        public abstract ElementVisitor AddProperty(Type propertyType, string propertyName, Expression getNewValue);
    }

    internal partial class ElementVisitor<TElement> : IElementVisitor<TElement>
    {
        private ExpressAccessor _elementAccessor;

        private ShallowCloner<TElement> _elementCloner;

        public override Type ElementType { get { return typeof(TElement); } }

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
                _elementAccessor = ExpressAccessor.Create(ownerType, typeof(TElement), elementName);
            }

            _elementCloner = new ShallowCloner<TElement>();
        }

        public object Extract(object parent)
        {
            return _elementAccessor.Get(parent);
        }

        public object SetCopy(object parent, object element)
        {
            var blueprint = _elementCloner.Clone((TElement)element);

            _elementAccessor.Set(parent, blueprint);

            return blueprint;
        }

        public void Visit(TElement element, TElement blueprint)
        {
            foreach (var propertyVisitor in PropertyVisitors)
            {
                propertyVisitor.Visit(element, blueprint);
            }

            foreach (var elementVisitor in ElementVisitors)
            {
                var childElement = elementVisitor.Extract(element);

                var childBlueprint = elementVisitor.SetCopy(blueprint, childElement);

                elementVisitor.Visit(childElement, childBlueprint);
            }
        }

        public void Visit(object element, object blueprint)
        {
            if (element == null)
            {
                return;
            }

            if (!(element is TElement && blueprint is TElement))
            {
                throw new Exception(string.Format("Given element and blueprint must be of type '{0}'", typeof(TElement).ToString()));
            }

            Visit((TElement)element, (TElement)blueprint);
        }
    }

    internal partial class ElementVisitor<TElement> : ElementVisitor
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

        public override ElementVisitor AddElement(Type elementType, string elementName)
        {
            var methodDef = typeof(ElementVisitor<TElement>).GetMethod("AddElementVisitor");
            var method = methodDef.MakeGenericMethod(elementType);
            var visitor = (ElementVisitor)method.Invoke(this, new[] { elementName });
            return visitor;
        }

        public IElementVisitor<TElement> AddPropertyVisitor<TProperty>(string propertyName, Expression<Func<TProperty, TProperty>> getNewValue)
        {
            if (PropertyVisitors.Any(pv => pv.ElementType == typeof(TElement) && pv.PropertyName == propertyName))
            {
                throw new ArgumentException(string.Format("Property visitor for type '{0}' and name '{1}' is already added!", typeof(TElement), propertyName));
            }

            var elementVisitor = new PropertyVisitor<TElement, TProperty>(propertyName, getNewValue);
            PropertyVisitors.Add(elementVisitor);
            return this;
        }

        public override ElementVisitor AddProperty(Type propertyType, string propertyName, Expression getNewValue)
        {
            var methodDef = typeof(ElementVisitor<TElement>).GetMethod("AddPropertyVisitor");
            var method = methodDef.MakeGenericMethod(propertyType);
            var visitor = (ElementVisitor)method.Invoke(this, new object[] { propertyName, getNewValue });
            return visitor;
        }
    }
}
