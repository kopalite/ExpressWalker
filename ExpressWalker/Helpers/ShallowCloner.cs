using System;
using System.Collections.Generic;
using ExpressWalker.Helpers;
using System.Linq.Expressions;

namespace ExpressWalker
{
    internal abstract class ShallowCloner
    {
        public abstract object Clone(object element);
    }

    internal sealed class ShallowCloner<TElement> : ShallowCloner
    {
        private Func<TElement> _constructor; 

        private List<ExpressAccessor> _accessors;

        public ShallowCloner()
        {
            //Creating constructor function (1st step: createing initial instance).

            _constructor = Constructor<TElement>();

            //creating property getter/setter for each property that is of primitive (value) type (2nd step: cloning primitive values into new instance)

            _accessors = new List<ExpressAccessor>();

            foreach (var property in typeof(TElement).GetProperties())
            {
                if (Util.IsSimpleType(property.PropertyType))
                {
                    var accessor = ExpressAccessor.Create(typeof(TElement), property.PropertyType, property.Name);

                    _accessors.Add(accessor);
                }
            }
        }

        public TElement Clone(TElement element)
        {
            if (element == null || element.Equals(default(TElement)))
            {
                return default(TElement);
            }

            var clone = _constructor();

            _accessors.ForEach(a =>
            {
                var value = a.Get(element);

                a.Set(clone, value);
            });

            return clone;
        }

        public override object Clone(object element)
        {
            if (element == null)
            {
                return null;
            }

            if (!(element is TElement))
            {
                throw new Exception(string.Format("Parameter 'element' must be of type '{0}'", typeof(TElement).Name));
            }

            return Clone((TElement)element);
        }

        private static Func<TEntity> Constructor<TEntity>()
        {
            var type = typeof(TEntity);

            if (HasParameterlessConstructor(type))
            {
                var body = Expression.New(type);
                var lambda = Expression.Lambda<Func<TEntity>>(body);
                return lambda.Compile();
            }
            else if (IsIEnumerable(type))
            {
                //TODO: return expression: "() => new[] { ... }"
            }
            else
            {
                //TODO: try to express constructor that accepts IEnumerable as parameter.
            }

            return null;
        }

        private static bool HasParameterlessConstructor(Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        private static bool IsIEnumerable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }
    }
}
