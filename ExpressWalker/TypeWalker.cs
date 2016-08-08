using ExpressWalker.Helpers;
using ExpressWalker.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressWalker
{
    public class TypeWalker<TRootType>
    {
        private List<PropertyTarget> _properties;

        private TypeWalker()
        {
            _properties = new List<PropertyTarget>();
        }

        public static TypeWalker<TRootType> Create()
        {
            return new TypeWalker<TRootType>();
        }

        public TypeWalker<TRootType> ForProperty<TPropertyType>(Expression<Func<TPropertyType, object, TPropertyType>> getNewValue)
        {
            _properties.Add(new PropertyTarget<TPropertyType>(null, typeof(TPropertyType), null, getNewValue));

            return this;
        }

        public TypeWalker<TRootType> ForProperty<TElementType, TPropertyType>(Expression<Func<TElementType, object>> propertyName,
                                                                              Expression<Func<TPropertyType, object, TPropertyType>> getNewValue)
        {
            _properties.Add(new PropertyTarget<TPropertyType>(typeof(TElementType), typeof(TPropertyType), Util.NameOf(propertyName), getNewValue));

            return this;
        }

        public IVisitor Build(int depth = Constants.MaxDepth)
        {
            var visitor = new ElementVisitor<TRootType>(null);
            Build(visitor, depth);
            return visitor;
        }

        private void Build(ElementVisitor visitor, int depth)
        {
            if (depth > Constants.MaxDepth)
            {
                throw new Exception(string.Format("Depth of visit cannot be more than {0}.", Constants.MaxDepth));
            }

            if (depth <= 0)
            {
                return;
            }

            var currentNodeType = visitor.ElementType;

            foreach (var property in currentNodeType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                //Trying to find property match, first by name, owner type and property type. If not found, we will try only with property type.

                var match = _properties.FirstOrDefault(p => p.ElementType == property.DeclaringType && p.PropertyName == property.Name && p.PropertyType == property.PropertyType);

                if (match == null)
                {
                    match = _properties.FirstOrDefault(p => p.ElementType == null && p.PropertyName == null && p.PropertyType == property.PropertyType);
                }

                if (match != null)
                {
                    visitor.AddProperty(property.PropertyType, property.Name, match.GetNewValue);
                }

                //If property type is not primitive, we will assume we should add it as an element, but after it's being built and turns out it's 'empty', we will remove it.

                if (!Util.IsSimpleType(property.PropertyType))
                {
                    var childVisitor = visitor.AddElement(property.PropertyType, property.Name);

                    Build(childVisitor, depth - 1);

                    if (!childVisitor.AnyElement && !childVisitor.AnyProperty)
                    {
                        visitor.RemoveElement(property.PropertyType, property.Name);
                    }
                }
            }
        }
    }
}
