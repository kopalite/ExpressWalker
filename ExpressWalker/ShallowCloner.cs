using System;
using System.Linq;
using System.Collections.Generic;

namespace ExpressWalker
{
    internal sealed class ShallowCloner<TElement>
    {
        private Func<TElement> _constructor; 

        private List<ExpressAccessor> _accessors;

        public ShallowCloner()
        {
            //Creating constructor function (1st step: createing initial instance).

            _constructor = Util.Constructor<TElement>();

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
    }
}
