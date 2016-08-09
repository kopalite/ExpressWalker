using ExpressWalker.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressWalker.Cloners
{
    internal sealed class ListCloner<TList, TItem> : ClonerBase
    {
        private Func<IEnumerable<TItem>, TList> _constructor;

        private ClonerBase _itemsCloner;

        public ListCloner() : base()
        {
            _constructor = Constructor();

            _itemsCloner = Create(typeof(TItem));
        }

        private TList Clone(TList list)
        {
            if (list == null || list.Equals(default(TList)))
            {
                return default(TList);
            }

            var items = ((IEnumerable<TItem>)list).Select(i => (TItem)_itemsCloner.Clone(i)).ToArray();

            var clone = _constructor(items);

            return clone;
        }

        public override object Clone(object element)
        {
            if (element == null)
            {
                return null;
            }

            if (!(element is TList))
            {
                throw new Exception(string.Format("Parameter 'element' must be of type '{0}'", typeof(TList).Name));
            }

            return Clone((TList)element);
        }

        private Func<IEnumerable<TItem>, TList> Constructor()
        {
            var type = typeof(TList);
            var ctor = Util.GetCollectionCtor(type, typeof(IEnumerable<TItem>));
            var input = Expression.Parameter(typeof(IEnumerable<TItem>));
            var body = Expression.New(ctor, input);
            var lambda = Expression.Lambda<Func<IEnumerable<TItem>, TList>>(body, input);
            return lambda.Compile();
        }
    }
}
