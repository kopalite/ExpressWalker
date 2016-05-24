using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressWalker
{
    public class TypeWalker<TRootType>
    {
        private List<ElementTarget> _elements;

        private List<PropertyTarget> _properties;

        private TypeWalker()
        {
            _elements = new List<ElementTarget>();

            _properties = new List<PropertyTarget>();
        }

        public static TypeWalker<TRootType> Create()
        {
            return new TypeWalker<TRootType>();
        }

        public TypeWalker<TRootType> ForElement<TElementType>()
        {
            _elements.Add(new ElementTarget(typeof(TElementType)));

            return this;
        }

        public TypeWalker<TRootType> ForProperty<TElementType, TPropertyType>(Expression<Func<TElementType, object>> propertyName,
                                                                              Expression<Func<TPropertyType, TPropertyType>> getNewValue)
        {
            _properties.Add(new PropertyTarget<TPropertyType>(typeof(TElementType), Exp.NameOf(propertyName), getNewValue));

            return this;
        }
        public IElementVisitor<TRootType> Build()
        {
            var visitor = new ElementVisitor<TRootType>(null);
            Build(visitor, 0);
            return visitor;
        }

        private void Build(ElementVisitor visitor, int depth)
        {
            if (depth == 1000)
            {
                throw new Exception("There is a circular references by type between properties!");
            }

            var currentNodeType = visitor.ElementType;

            foreach (var property in currentNodeType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyMatch = _properties.FirstOrDefault(p => p.ElementType == property.DeclaringType && p.PropertyName == property.Name);
                if (propertyMatch != null)
                {
                    visitor.AddProperty(property.PropertyType, property.Name, propertyMatch.GetNewValue);
                }

                var elementMatch = _elements.FirstOrDefault(t => t.ElementType == property.PropertyType);
                if (elementMatch != null)
                {
                    var childVisitor = visitor.AddElement(property.PropertyType, property.Name);

                    Build(childVisitor, depth + 1);
                }
            }

            //TODO: Build recursivelly a IElementVisitor<TRootType> using ElementVisitor methods - basing on:

            //1. TRootType type
            //2. Elements list
            //3. Properties list

            //hint: use ElementVisitor abstract class non-generic methods: AddElement() and AddProperty() - they accept types :).
        }
    }
}
