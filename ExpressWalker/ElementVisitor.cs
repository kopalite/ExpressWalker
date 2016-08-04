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

        void Visit(object element, object blueprint = null, int depth = Constants.MaxDepth, InstanceGuard guard = null);
    }

    public interface IElementVisitor<TElement> : IElementVisitor where TElement : class, new()
    {

    }

    internal abstract class ElementVisitor
    {
        public abstract Type ElementType { get; }

        public abstract string ElementName { get; protected set; }

        public abstract bool AnyElement { get; }

        public abstract bool AnyProperty { get; }

        public abstract ElementVisitor AddElement(Type elementType, string elementName);

        public abstract void RemoveElement(Type elementType, string elementName);

        //getNewValue is convertible to Expression<Func<TPropertyType, TPropertyType>> where TPropertyType is specified in derived class.
        //getOldValue is convertible to Expression<Action<TPropertyType>> where TPropertyType is specified in derived class.
        public abstract ElementVisitor AddProperty(Type propertyType, string propertyName, Expression getOldValue, Expression getNewValue);
    }

    internal partial class ElementVisitor<TElement> : IElementVisitor<TElement> where TElement : class, new()
    {
        private readonly ExpressAccessor _elementAccessor;

        private readonly ShallowCloner<TElement> _elementCloner;
        
        private readonly HashSet<IElementVisitor> _elementVisitors;

        private readonly HashSet<IPropertyVisitor<TElement>> _propertyVisitors;

        public override Type ElementType { get { return typeof(TElement); } }

        public override string ElementName { get; protected set; }

        public override bool AnyElement { get { return _elementVisitors.Any(); } }

        public override bool AnyProperty { get { return _propertyVisitors.Any(); } }

        public ElementVisitor(Type ownerType) : this(ownerType, null)
        {
            
        }

        public ElementVisitor(Type ownerType, string elementName) 
        {
            _elementVisitors = new HashSet<IElementVisitor>();

            _propertyVisitors = new HashSet<IPropertyVisitor<TElement>>();

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

        public void Visit(object element, object blueprint = null, int depth = Constants.MaxDepth, InstanceGuard guard = null)
        {
            if (element == null)
            {
                return;
            }

            if (!(element is TElement))
            {
                throw new Exception(string.Format("Given element and must be of type '{0}'", typeof(TElement).ToString()));
            }
            

            if (blueprint != null && (!(blueprint is TElement)))
            {
                throw new Exception(string.Format("Given blueprint must be of type '{0}'", typeof(TElement).ToString()));
            }

            Visit((TElement)element, (TElement)blueprint, depth, guard);
        }

        public void Visit(TElement element, TElement blueprint = null, int depth = Constants.MaxDepth, InstanceGuard guard = null)
        {
            if (depth > Constants.MaxDepth)
            {
                throw new Exception(string.Format("Depth of visit cannot be more than {0}.", Constants.MaxDepth));
            }

            //If the depth reached given maximum at begining or instance was already visited (we have circular reference), we will just return.

            if (depth <= 0 || (guard != null && guard.IsGuarded(element)))
            {
                return;
            }

            //Protecting the instance to be visited again.

            if (guard != null)
            {
                guard.Guard(element);
            }

            //Visiting properties.

            foreach (var propertyVisitor in _propertyVisitors)
            {
                propertyVisitor.Visit(element, blueprint);
            }

            //Visiting elements.

            foreach (var elementVisitor in _elementVisitors)
            {
                var childElement = elementVisitor.Extract(element);

                object childBlueprint = null;

                if (blueprint != null)
                {
                    childBlueprint = elementVisitor.SetCopy(blueprint, childElement);
                }

                //Setting the InstanceGuard of child element visitor with already visited instances.

                elementVisitor.Visit(childElement, childBlueprint, depth - 1, guard);
            }
        }
    }

    internal partial class ElementVisitor<TElement> : ElementVisitor
    {
        public IElementVisitor<TChildElement> AddElementVisitor<TChildElement>(string elementName) where TChildElement : class, new()
        {
            if (_elementVisitors.Any(ev => ev.ElementType == typeof(TChildElement) && ev.ElementName == elementName))
            {
                throw new ArgumentException(string.Format("Element visitor for type '{0}' and name '{1}' is already added!", typeof(TElement), elementName));
            }

            var elementVisitor = new ElementVisitor<TChildElement>(typeof(TElement), elementName);
            _elementVisitors.Add(elementVisitor);
            return elementVisitor;
        }

        public override ElementVisitor AddElement(Type elementType, string elementName)
        {
            var methodDef = typeof(ElementVisitor<TElement>).GetMethod("AddElementVisitor");
            var method = methodDef.MakeGenericMethod(elementType);
            var visitor = (ElementVisitor)method.Invoke(this, new[] { elementName });
            return visitor;
        }

        public void RemoveElementVisitor<TChildElement>(string elementName)
        {
            var elementVisitor = _elementVisitors.FirstOrDefault(ev => ev.ElementType == typeof(TChildElement) && ev.ElementName == elementName);
            if (elementVisitor == null)
            {
                throw new ArgumentException(string.Format("Element visitor for type '{0}' and name '{1}' is not found!", typeof(TElement), elementName));
            }
            _elementVisitors.Remove(elementVisitor);
        }

        public override void RemoveElement(Type elementType, string elementName)
        {
            var methodDef = typeof(ElementVisitor<TElement>).GetMethod("RemoveElementVisitor");
            var method = methodDef.MakeGenericMethod(elementType);
            method.Invoke(this, new[] { elementName });
            
        }

        public IElementVisitor<TElement> AddPropertyVisitor<TProperty>(string propertyName, Expression<Action<TProperty, object>> getOldValue, Expression<Func<TProperty, object, TProperty>> getNewValue)
        {
            if (_propertyVisitors.Any(pv => pv.ElementType == typeof(TElement) && pv.PropertyName == propertyName))
            {
                throw new ArgumentException(string.Format("Property visitor for type '{0}' and name '{1}' is already added!", typeof(TElement), propertyName));
            }

            var propertyVisitor = new PropertyVisitor<TElement, TProperty>(propertyName, getOldValue, getNewValue, GetPropertyMetadata(propertyName));
            _propertyVisitors.Add(propertyVisitor);
            return this;
        }

        public override ElementVisitor AddProperty(Type propertyType, string propertyName, Expression getOldValue, Expression getNewValue)
        {
            var methodDef = typeof(ElementVisitor<TElement>).GetMethod("AddPropertyVisitor");
            var method = methodDef.MakeGenericMethod(propertyType);
            var visitor = (ElementVisitor)method.Invoke(this, new object[] { propertyName, getOldValue, getNewValue });
            return visitor;
        }

        private object GetPropertyMetadata(string propertyName)
        {
            var elementType = typeof(TElement);
            var property = elementType.GetProperty(propertyName);
            var metadataAttribute = (VisitorMetadataAttribute)property.GetCustomAttributes(typeof(VisitorMetadataAttribute), false).FirstOrDefault();
            return (metadataAttribute == null) ? null : metadataAttribute.Metadata;
        }
    }
}
