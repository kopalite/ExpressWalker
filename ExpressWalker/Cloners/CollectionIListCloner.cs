using ExpressWalker.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressWalker.Cloners
{
    internal sealed class CollectionIListCloner<TCollection, TItem> : ShallowCloner
    {
        private Func<IList<TItem>, TCollection> _constructor;

        private ShallowCloner _itemsCloner;

        public CollectionIListCloner()
        {
            _constructor = Constructor();

            _itemsCloner = Create(typeof(TItem));
        }

        private TCollection Clone(TCollection collection)
        {
            if (collection == null || collection.Equals(default(TCollection)))
            {
                return default(TCollection);
            }

            var items = ((IList<TItem>)collection).Select(i => (TItem)_itemsCloner.Clone(i)).ToArray();

            var clone = _constructor(items);

            return clone;
        }

        public override object Clone(object element)
        {
            if (element == null)
            {
                return null;
            }

            if (!(element is TCollection))
            {
                throw new Exception(string.Format("Parameter 'element' must be of type '{0}'", typeof(TCollection).Name));
            }

            return Clone((TCollection)element);
        }

        private Func<IList<TItem>, TCollection> Constructor()
        {
            var type = typeof(TCollection);
            var ctor = Util.GetCollectionCtor(type, typeof(IList<TItem>));
            var input = Expression.Parameter(typeof(IList<TItem>));
            var body = Expression.New(ctor, input);
            var lambda = Expression.Lambda<Func<IList<TItem>, TCollection>>(body, input);
            return lambda.Compile();
        }
    }
}
