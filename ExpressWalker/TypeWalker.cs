﻿using ExpressWalker.Helpers;
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

        public IVisitor Build(int depth = Constants.MaxDepth, PropertyGuard guard = null)
        {
            var visitor = new ElementVisitor<TRootType>(null, null, guard);
            Build(visitor, depth);
            return visitor;
        }

        private void Build(ElementVisitor visitor, int depth)
        {
            if (depth > Constants.MaxDepth)
            {
                throw new Exception(string.Format("Depth of visit cannot be more than {0}.", Constants.MaxDepth));
            }

            if (depth < 0)
            {
                return;
            }

            var currentNodeType = visitor.ElementType;

            foreach (var prop in ReflectionCache.GetData(currentNodeType).Properties.Values)
            {
                //Trying to find property match, first by name, owner type and property type. If not found, we will try only with property type.

                var match = _properties.FirstOrDefault(p => p.ElementType == prop.DeclaringType.RawType && p.PropertyName == prop.PropertyName && p.PropertyType == prop.PropertyType.RawType);

                if (match == null)
                {
                    match = _properties.FirstOrDefault(p => p.ElementType == null && p.PropertyName == null && p.PropertyType == prop.PropertyType.RawType);
                }

                if (match != null)
                {
                    visitor.AddProperty(prop.PropertyType.RawType, prop.PropertyName, match.GetNewValue);
                }

                if (prop.PropertyType.IsSimpleType)
                {
                    continue;
                }

                //If property type is not primitive, we will assume we should add it as an element/collection, but after it's being built and turns out it's 'empty', we will remove it.

                if (prop.PropertyType.IsDictionary)
                {
                    //TODO: Finish dictionary visit configuration once it's supported.
                }
                else if (prop.PropertyType.IsGenericEnumerable || prop.PropertyType.ImplementsGenericIEnumerable)
                {
                    if (prop.PropertyType.IsCollectionItemsTypeSimple)
                    {
                        continue;
                    }

                    var childVisitor = visitor.AddCollection(prop.PropertyType.CollectionItemsType, prop.PropertyType.RawType, prop.PropertyName, prop.IsVisitorHierarchy);

                    if (childVisitor == null) //AddCollection() will return null in case of issues like circular reference.
                    {
                        continue;
                    }

                    Build(childVisitor, depth - 1);

                    if (!childVisitor.AnyElement && !!childVisitor.AnyCollection && !childVisitor.AnyProperty)
                    {
                        visitor.RemoveCollection(prop.PropertyType.RawType, prop.PropertyName);
                    }
                }
                else
                {
                    var childVisitor = visitor.AddElement(prop.PropertyType.RawType, prop.PropertyName, prop.IsVisitorHierarchy);

                    if (childVisitor == null) //AddElement() will return null in case of issues like circular reference.
                    {
                        continue;
                    }

                    Build(childVisitor, depth - 1);

                    if (!childVisitor.AnyElement && !childVisitor.AnyCollection && !childVisitor.AnyProperty)
                    {
                        visitor.RemoveElement(prop.PropertyType.RawType, prop.PropertyName);
                    }
                }
            }
        }
    }
}
