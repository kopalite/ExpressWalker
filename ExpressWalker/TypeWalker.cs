using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressWalker
{
    public class TypeWalker<TRootType>
    {
        public int Depth { get; }

        private List<ElementTarget> _elements;

        private List<PropertyTarget> _properties;

        private TypeWalker(int depth)
        {
            Depth = depth;

            _elements = new List<ElementTarget>();

            _properties = new List<PropertyTarget>();
        }

        public static TypeWalker<TRootType> Create(int depth)
        {
            return new TypeWalker<TRootType>(depth);
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
            //TODO: Build recursivelly a IElementVisitor<TRootType> using ElementVisitor methods - basing on:

            //1. TRootType type
            //2. Elements list
            //3. Properties list

            //hint: use ElementVisitor abstract class non-generic methods: AddElement() and AddProperty() - they accept types :).
        }
    }
}
