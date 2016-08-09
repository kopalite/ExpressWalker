using ExpressWalker.Cloners;
using ExpressWalker.Visitors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressWalker
{
    internal abstract class ElementVisitor
    {
        public abstract Type ElementType { get; }

        public abstract string ElementName { get; protected set; }

        public abstract bool AnyElement { get; }

        public abstract bool AnyCollection { get; }

        public abstract bool AnyProperty { get; }

        public abstract ElementVisitor AddElement(Type elementType, string childElementName);

        public abstract void RemoveElement(Type elementType, string childElementName);

        public abstract ElementVisitor AddCollection(Type elementType, Type collectionType, string collectionName);

        public abstract void RemoveCollection(Type collectionType, string collectionName);

        //getNewValue is convertible to Expression<Func<TPropertyType, TPropertyType>> where TPropertyType is specified in derived class.
        public abstract ElementVisitor AddProperty(Type propertyType, string propertyName,  Expression getNewValue);
    }

    internal partial class ElementVisitor<TElement> : IElementVisitor<TElement>
    {
        protected ExpressAccessor _elementAccessor;

        protected ClonerBase _elementCloner;

        protected HashSet<IElementVisitor> _elementVisitors;

        protected HashSet<IElementVisitor> _collectionVisitors;

        protected HashSet<IPropertyVisitor<TElement>> _propertyVisitors;

        public override Type ElementType { get { return typeof(TElement); } }

        public override string ElementName { get; protected set; }

        public override bool AnyElement { get { return _elementVisitors.Any(); } }

        public override bool AnyCollection { get { return _collectionVisitors.Any(); } }

        public override bool AnyProperty { get { return _propertyVisitors.Any(); } }

        protected ElementVisitor()
        {
            _elementVisitors = new HashSet<IElementVisitor>();

            _collectionVisitors = new HashSet<IElementVisitor>();

            _propertyVisitors = new HashSet<IPropertyVisitor<TElement>>();
        }

        public ElementVisitor(Type ownerType) : this(ownerType, null)
        {
            
        }

        public ElementVisitor(Type ownerType, string elementName) : this()
        {
            ElementName = elementName;

            if (!string.IsNullOrWhiteSpace(elementName))
            {
                _elementAccessor = ExpressAccessor.Create(ownerType, typeof(TElement), elementName);
            }

            _elementCloner = ClonerBase.Create(typeof(TElement));
        }

        public object Extract(object parent)
        {
            return _elementAccessor.Get(parent);
        }

        public object SetCopy(object parent, object element)
        {
            var blueprint = _elementCloner.Clone(element);

            _elementAccessor.Set(parent, blueprint);

            return blueprint;
        }

        public void Visit(object element, object blueprint = null, int depth = Constants.MaxDepth, InstanceGuard guard = null, HashSet<PropertyValue> values = null)
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

            Visit((TElement)element, (TElement)blueprint, depth, guard, values);
        }

        public void Visit(TElement element, TElement blueprint = default(TElement), int depth = Constants.MaxDepth, InstanceGuard guard = null, HashSet<PropertyValue> values = null)
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
                var value = propertyVisitor.Visit(element, blueprint);

                if (values != null)
                {
                    values.Add(value);
                }
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

                elementVisitor.Visit(childElement, childBlueprint, depth - 1, guard, values);
            }

            //Visiting collections.

            foreach (var collectionVisitor in _collectionVisitors)
            {
                var childCollection = (IEnumerable)collectionVisitor.Extract(element);

                IEnumerable childCollectionBlueprint = null;

                if (blueprint != null)
                {
                    childCollectionBlueprint = (IEnumerable)collectionVisitor.SetCopy(blueprint, childCollection);
                }

                if (childCollection == null)
                {
                    continue;
                }

                var originalEnumerator = childCollection.GetEnumerator();
                var blueprintEnumerator = childCollectionBlueprint.GetEnumerator();

                while (originalEnumerator.MoveNext() && blueprintEnumerator.MoveNext())
                {
                    var originalElement = originalEnumerator.Current;
                    var blueprintElement = blueprintEnumerator.Current;

                    collectionVisitor.Visit(originalElement, blueprintElement, depth - 1, guard, values);
                }
            }
        }
    }

    internal partial class ElementVisitor<TElement> : ElementVisitor
    {
        internal IElementVisitor<TChildElement> AddElementVisitor<TChildElement>(string childElementName)
        {
            if (_elementVisitors.Any(ev => ev.ElementType == typeof(TChildElement) && ev.ElementName == childElementName))
            {
                throw new ArgumentException(string.Format("Element visitor for type '{0}' and name '{1}' is already added!", typeof(TElement), childElementName));
            }

            var elementVisitor = new ElementVisitor<TChildElement>(typeof(TElement), childElementName);
            _elementVisitors.Add(elementVisitor);
            return elementVisitor;
        }

        public override ElementVisitor AddElement(Type elementType, string childElementName)
        {
            var methodDef = typeof(ElementVisitor<TElement>).GetMethod("AddElementVisitor", BindingFlags.NonPublic | BindingFlags.Instance);
            var method = methodDef.MakeGenericMethod(elementType);
            var visitor = (ElementVisitor)method.Invoke(this, new[] { childElementName });
            return visitor;
        }

        internal void RemoveElementVisitor<TChildElement>(string childElementName)
        {
            var elementVisitor = _elementVisitors.FirstOrDefault(ev => ev.ElementType == typeof(TChildElement) && ev.ElementName == childElementName);
            if (elementVisitor == null)
            {
                throw new ArgumentException(string.Format("Element visitor for type '{0}' and name '{1}' is not found!", typeof(TElement), childElementName));
            }
            _elementVisitors.Remove(elementVisitor);
        }

        public override void RemoveElement(Type elementType, string childElementName)
        {
            var methodDef = typeof(ElementVisitor<TElement>).GetMethod("RemoveElementVisitor", BindingFlags.NonPublic | BindingFlags.Instance);
            var method = methodDef.MakeGenericMethod(elementType);
            method.Invoke(this, new[] { childElementName });
            
        }

        internal IElementVisitor<TChildElement> AddCollectionVisitor<TChildElement>(Type collectionType, string collectionName)
        {
            if (_collectionVisitors.Any(ev => ev.ElementType == typeof(TChildElement) && ev.ElementName == collectionName))
            {
                throw new ArgumentException(string.Format("Collection visitor for type '{0}' and name '{1}' is already added!", typeof(TElement), collectionName));
            }

            var collectionVisitor = new CollectionVisitor<TChildElement>(typeof(TElement), collectionType, collectionName);
            _collectionVisitors.Add(collectionVisitor);
            return collectionVisitor;
        }

        public override ElementVisitor AddCollection(Type elementType, Type collectionType, string collectionName)
        {
            var methodDef = typeof(ElementVisitor<TElement>).GetMethod("AddCollectionVisitor", BindingFlags.NonPublic | BindingFlags.Instance);
            var method = methodDef.MakeGenericMethod(elementType);
            var visitor = (ElementVisitor)method.Invoke(this, new object[] { collectionType, collectionName });
            return visitor;
        }

        internal void RemoveCollectionVisitor<TChildElement>(string collectionName)
        {
            var collectionVisitor = _collectionVisitors.FirstOrDefault(ev => ev.ElementType == typeof(TChildElement) && ev.ElementName == collectionName);
            if (collectionVisitor == null)
            {
                throw new ArgumentException(string.Format("Collection visitor for type '{0}' and name '{1}' is not found!", typeof(TElement), collectionName));
            }
            _collectionVisitors.Remove(collectionVisitor);
        }

        public override void RemoveCollection(Type elementType, string collectionName)
        {
            var methodDef = typeof(ElementVisitor<TElement>).GetMethod("RemoveCollectionVisitor", BindingFlags.NonPublic | BindingFlags.Instance);
            var method = methodDef.MakeGenericMethod(elementType);
            method.Invoke(this, new[] { collectionName });
        }

        internal IElementVisitor<TElement> AddPropertyVisitor<TProperty>(string propertyName, Expression<Func<TProperty, object, TProperty>> getNewValue)
        {
            if (_propertyVisitors.Any(pv => pv.ElementType == typeof(TElement) && pv.PropertyName == propertyName))
            {
                throw new ArgumentException(string.Format("Property visitor for type '{0}' and name '{1}' is already added!", typeof(TElement), propertyName));
            }

            var propertyVisitor = new PropertyVisitor<TElement, TProperty>(propertyName, getNewValue, GetPropertyMetadata(propertyName));
            _propertyVisitors.Add(propertyVisitor);
            return this;
        }

        public override ElementVisitor AddProperty(Type propertyType, string propertyName, Expression getNewValue)
        {
            var methodDef = typeof(ElementVisitor<TElement>).GetMethod("AddPropertyVisitor", BindingFlags.NonPublic | BindingFlags.Instance);
            var method = methodDef.MakeGenericMethod(propertyType);
            var visitor = (ElementVisitor)method.Invoke(this, new object[] { propertyName, getNewValue });
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
